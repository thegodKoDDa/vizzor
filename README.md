# 🧊 Vizzor

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)]()
[![Hardware](https://img.shields.io/badge/Hardware-Rise%20Mode%20Aura%20Ice-00d4ff.svg)]()
[![Size](https://img.shields.io/badge/Size-19%20KB-success.svg)]()

> Software alternativo, leve e de código aberto para o visor LCD do water cooler **Rise Mode Aura Ice Black ARGB 360 mm** (`RM-WAIB-06-ARGB`).

O software original do fabricante é notório por problemas de estabilidade, alto consumo de memória RAM (> 100 MB com 3 processos ativos) e limitação de só exibir a temperatura da CPU. O **Vizzor** foi desenvolvido do zero via engenharia reversa do protocolo USB HID para oferecer uma solução estável, com suporte a **GPU (NVIDIA/AMD)**, painel visual moderno e consumo inferior a 45 MB de RAM.

---

## ⚡ Comparativo: Software Oficial vs Vizzor

| Recurso | Software Oficial (Rise Mode) | Vizzor v3.0 |
| :--- | :---: | :---: |
| **Exibição de Temperatura da GPU** | ❌ Não suportado | ✅ **Sim (NVIDIA & AMD)** |
| **Modo Hotspot (Mais Quente)** | ❌ Não | ✅ **Sim (CPU vs GPU dinâmico)** |
| **Alternância Automática** | ❌ Não | ✅ **Sim (a cada 3s, 5s ou 10s)** |
| **Modo Relógio de Gabinete** | ❌ Não | ✅ **Sim (Minutos no display)** |
| **Modo Stealth (Apagar Visor)** | ❌ Não | ✅ **Sim (Via menu da bandeja)** |
| **Animação de Boot (Corrida)** | ❌ Não | ✅ **Sim (Ao inicializar)** |
| **Alerta Visual Térmico no Visor**| ❌ Não | ✅ **Sim (Pisca visor se > 85°C)** |
| **Ícone Dinâmico na Barra de Tarefas**| ❌ Não (ícone estático) | ✅ **Sim (Temperatura ao vivo com cores térmicas)** |
| **Painel de Controle Visual** | ❌ Interface pesada/antiga | ✅ **Dashboard Dark Mode moderno** |
| **Consumo de Memória RAM** | ⚠️ ~110 MB (3 processos) | 🚀 **~40 MB (Processo único)** |
| **Tamanho do Executável** | ⚠️ ~30 MB de binários | 🚀 **19 KB (.NET nativo)** |

---

## 🖥️ Recursos e Funcionalidades

### 1. Painel de Controle (Dashboard Dark Mode)
Dê **dois cliques** no ícone da bandeja para abrir o painel de controle. Ele inclui:
* Réplica visual estilizada do cabeçote do water cooler em tempo real;
* Cards com telemetria detalhada de CPU (Ryzen SMU), GPU (NVAPI/AMD) e RAM;
* Seleção rápida de modos de exibição e controle do display.

### 2. Ícone Térmico ao Vivo na Barra de Tarefas
O ícone do Vizzor ao lado do relógio do Windows atualiza o número da temperatura em tempo real com cores térmicas inteligentes:
* 🔵 **Ciano:** Temperatura normal (< 65 °C)
* 🟠 **Laranja:** Carga moderada / Jogos (65 °C – 80 °C)
* 🔴 **Vermelho:** Alerta térmico (> 80 °C)

### 3. Efeitos Visuais no Water Cooler
* **Bootup Sequence:** Ao iniciar o PC, o visor executa uma animação de contagem de corrida rápida (`00 -> 99`) antes de fixar na temperatura real.
* **Alerta Visual no Gabinete:** Se o processador ou placa de vídeo atingir 85 °C, o visor físico pisca no vidro do gabinete para alertar o usuário.
* **Modo Stealth:** Permite apagar 100% o visor do cooler para dormir ou assistir filmes no escuro.

---

## 🚀 Instalação Rápida

1. Baixe o pacote mais recente em [Releases](dist/Vizzor_v3.0_Portable.zip).
2. Extraia o arquivo `.zip`.
3. Execute o `Vizzor.exe` (ou use o `Instalar.bat` para criar o atalho automático na Área de Trabalho e inicialização do Windows).
4. *(Opcional)* Clique com o botão direito no ícone da bandeja e selecione **"Desativar Software Antigo da Rise Mode"** para remover o serviço `tifSer`.

---

## 🔬 Especificação Técnica do Protocolo USB HID

Para desenvolvedores e entusiastas de hardware, o protocolo do visor foi decodificado:

* **Vendor ID (VID):** `0xAA88` (`43656`)
* **Product ID (PID):** `0x8666` (`34406`)
* **Dispositivo USB:** `温度显示HID设备` (Fabricante: `铭研科技` - Mingyan Technology)
* **Estrutura do Pacote HID:** Relatório de saída direto com **11 bytes** (Report ID `0x00`):

```text
Byte 0:  0x00 (Report ID)
Byte 1:  Valor Exibido no Visor (0 a 99) -> Temperatura em °C, Porcentagem ou Minuto
Byte 2:  Temperatura da GPU (°C)
Byte 3:  Uso da CPU (%)
Byte 4:  Uso de Memória RAM (%)
Byte 5:  Temperatura da Placa-Mãe (°C)
Byte 6:  Uso da GPU (%)
Bytes 7-10: Reservado (0x00)
```

> **Watchdog do Display:** O microcontrolador do display possui um timeout de ~2 a 3 segundos. Se não receber pacotes periódicos, ele desliga a tela automaticamente para evitar que dados desatualizados permaneçam congelados.

Documentação técnica completa em [`docs/PROTOCOL.md`](docs/PROTOCOL.md).

---

## 🛠️ Compilação

Para compilar localmente utilizando o compilador C# nativo do Windows:

```cmd
csc.exe /target:winexe /out:Vizzor.exe /win32icon:app.ico /win32manifest:app.manifest /r:LibreHardwareMonitorLib.dll /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll *.cs
```

---

## 📄 Licença

Distribuído sob a licença **MIT**. Veja [`LICENSE`](LICENSE) para mais detalhes.
