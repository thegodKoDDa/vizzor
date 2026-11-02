# Contexto Histórico e Evidências Técnicas - Vizzor

Este documento registra a análise de engenharia reversa dos binários legados da Rise Mode e a evolução técnica que culminou na criação do software **Vizzor**.

---

## 1. O Problema Original dos Softwares Proprietários

O software oficial fornecido originalmente para o controle do visor do water cooler **Rise Mode Aura Ice Black ARGB 360 mm** (`RM-WAIB-06-ARGB`) apresentava severas deficiências arquiteturais:

* **Arquitetura Fragmentada em 3 Processos:**
  1. `Rise Mode Temp CPU Driver R2.2.exe` (Aplicação WinForms pesada);
  2. `CPU Server.exe` (Serviço Windows `tifSer` em execução contínua como `LocalSystem`);
  3. `SendTemp.exe` (Processo filho gerado pelo serviço para enviar dados USB HID).
* **Consumo de Memória Exorbitante:** Mais de 110 MB de memória RAM combinada para realizar uma tarefa trivial de envio de 11 bytes a cada segundo.
* **Limitação de Recursos:** Exibia estritamente a temperatura da CPU, ignorando completamente placas de vídeo dedicadas (GPU NVIDIA GeForce e AMD Radeon), memórias e SSDs.
* **Incompatibilidade e Travamentos com AMD Ryzen:** Falha frequente de leitura que travava o mostrador em 0 °C por falta de comunicação com o SMU do processador.

---

## 2. Descoberta e Engenharia Reversa do Protocolo USB HID

Através da descompilação e inspeção estática dos assemblies .NET e análise dos pipes nomeados (`tifPipe`), identificamos os parâmetros exatos do controlador físico:

* **Fabricante OEM:** Mingyan Technology (`铭研科技`)
* **Vendor ID (VID):** `0xAA88` (`43656`)
* **Product ID (PID):** `0x8666` (`34406`)
* **Dispositivo USB String:** `温度显示HID设备`
* **Relatório de Saída (Output Report):** 11 bytes (Report ID `0x00` + 10 bytes de payload térmico/carga).
* **Watchdog de Segurança:** O microcontrolador físico do display requer o recebimento de pacotes HID a cada 2 a 3 segundos. Se a aplicação parar de enviar, o display se desliga para economizar energia.

---

## 3. O Papel do Driver de Kernel PawnIO

Para obter dados de temperatura confiáveis da CPU em sistemas Windows modernos de 64 bits (Ring 3 não possui acesso privilegiado a registradores MSR ou SMU), o ecossistema utiliza o driver assinado de kernel **PawnIO** (`PawnIO.sys`, desenvolvido originalmente pela Keroro Software LLC).
* O **PawnIO** faz a ponte segura entre o kernel do sistema operacional e o microcontrolador SMU da AMD / registradores MSR da Intel.
* O **Vizzor** lê esses sensores via `LibreHardwareMonitorLib.dll` e transmite instantaneamente ao visor.

---

## 4. Evolução para o Vizzor Multi-Cooler

A partir da versão v3.0, o projeto foi unificado em um único executável nativo Windows em C# .NET Framework 4.8 (~69 KB), evoluindo na v3.1 e v3.2 para um motor modular de detecção automática para coolers de diversas marcas:
* **Rise Mode / Mingyan OEM**
* **DeepCool Digital Series (AK400, AK500, AK620, CH560)**
* **NZXT Kraken Series (Z / Elite / 2023)**
* **Lian Li Series (Galahad II LCD / TL LCD)**
* **Cooler Master Series (MasterLiquid Atmos)**
