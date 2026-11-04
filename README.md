# 🧊 Vizzor

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)]()
[![Release](https://img.shields.io/badge/Release-v3.4-success.svg)](https://github.com/thegodKoDDa/vizzor/releases/latest)
[![Size](https://img.shields.io/badge/Size-~1.4%20MB%20(DLLs%20embutidas)-brightgreen.svg)]()
[![Language](https://img.shields.io/badge/i18n-PT--BR%20%7C%20EN--US-orange.svg)]()

> **Software aberto, independente e ultraleve para displays e visores de water coolers e coolers digitais.**  
> *Open-source, ultra-lightweight hardware monitor & display controller for AIO water coolers and digital air coolers.*

O **Vizzor** foi criado para substituir os softwares proprietários fornecidos pelos fabricantes (como o da Rise Mode e outros utilitários pesados), que frequentemente sofrem de travamentos constantes, consomem mais de 110 MB de RAM em múltiplos processos e limitam o visor a exibir apenas a temperatura da CPU.

Construído via engenharia reversa direta sobre o protocolo **USB HID**, o Vizzor utiliza **.NET Framework 4.8**, mantém a leitura da GPU em um **processo auxiliar isolado**, suporta múltiplos fabricantes e permite monitorar **CPU, GPU (NVIDIA & AMD), Memória RAM e SSD NVMe M.2**.

## 🛠️ Novidades da v3.4

* **CPU:** dependências de telemetria completas no EXE, seleção do sensor independente da ordem e `N/D` quando não há temperatura válida;
* **Cooler USB:** busca automática a cada cinco segundos, incluindo conexão tardia e reconexão;
* **Inicialização com o Windows:** tarefa agendada interativa com privilégios elevados e atraso de 15 segundos após o login;
* **Estabilidade NVIDIA:** a falha do processo auxiliar de GPU não encerra a CPU, a interface nem o envio ao visor;
* **Projeto unificado:** uma única árvore em `src/Vizzor`, uma distribuição em `dist`, atualizações sem backups automáticos e build que remove seus temporários.

**É o mesmo programa com ou sem cooler:** o modo monitor de PC é ativado automaticamente quando nenhum dispositivo suportado está conectado. O nome do executável não altera o suporte USB.

---

## 🧊 Hardware Suportado (Multi-Cooler Engine)

O Vizzor possui um motor de **Detecção Automática (Auto-Detect)** que procura dispositivos USB ao iniciar e repete a busca a cada cinco segundos quando não há conexão ativa:

| Fabricante / Marca | Modelos Suportados | Identificação USB | Status |
| :--- | :--- | :---: | :---: |
| **Rise Mode** | Aura Ice Black & White ARGB 240 mm / 360 mm (`RM-WAIB-06-ARGB`) | `VID: 0xAA88`, `PID: 0x8666` | ✅ Validado no Hardware Real |
| **SuperFrame / Alseye** | Coolers Digitais OEM Mingyan | `VID: 0xAA88`, `PID: 0x8667`, `0x8668` | 🧩 Driver implementado |
| **DeepCool** | AK400 Digital, AK500 Digital, AK620 Digital, CH560 Digital | `VID: 0x3633`, `PID: 0x0001`, `0x0002` | 🧩 Driver implementado |
| **NZXT** | Kraken Z53/Z63/Z73, Kraken Elite 240/280/360, Kraken 2023 | `VID: 0x1E71`, `PID: 0x2007`, `0x3008`, `0x2001` | 🧩 Driver implementado |
| **Lian Li** | Galahad II LCD, UNI FAN TL LCD | `VID: 0x0CF2`, `PID: 0xA102` | 🧩 Driver implementado |
| **Cooler Master** | MasterLiquid 360 Atmos / LCD Series | `VID: 0x2516`, `PID: 0x1001` | 🧩 Driver implementado |

O Rise Mode Aura Ice foi validado em hardware real. Os demais drivers dependem do modelo e do protocolo do dispositivo; a implementação não equivale à validação física de todos os modelos.

### 🔍 Scanner e Diagnóstico USB Integrado
Tem um water cooler diferente ou não sabe o modelo exato?
1. Abra o painel do Vizzor e clique no botão **`🔍 Diagnóstico USB`**;
2. O sistema listará automaticamente todos os dispositivos USB HID conectados, fabricantes e IDs (`VID` e `PID`);
3. Clique em **`📋 Copiar Relatório`** e cole em uma **[Nova Issue no GitHub](https://github.com/thegodKoDDa/vizzor/issues)** para adicionarmos suporte oficial ao seu cooler!

---

## ⚡ Comparativo: Software Oficial vs Vizzor

| Recurso | Softwares Proprietários Legados | Vizzor v3.4 |
| :--- | :---: | :---: |
| **Temperatura da GPU** | ❌ Não suportado | ✅ **Sim (NVIDIA GeForce & AMD Radeon)** |
| **Temperatura do SSD NVMe M.2** | ❌ Não | ✅ **Sim (Telemetria ao vivo e no display)** |
| **Modo Standalone (PC Monitor)** | ❌ Não | ✅ **Sim (Funciona em qualquer PC sem cooler USB)** |
| **Telemetria de Clocks, Watts e Fans** | ❌ Não | ✅ **Sim (CPU GHz/Watts, GPU VRAM/Fans, Placa-Mãe)** |
| **Modo Hotspot (O Mais Quente)** | ❌ Não | ✅ **Sim (Compara CPU e GPU em tempo real)** |
| **Alternância Automática** | ❌ Não | ✅ **Sim (Alterna sensores a cada 5s)** |
| **Modo Relógio no Visor** | ❌ Não | ✅ **Sim (Exibe os minutos atuais)** |
| **Modo Stealth (Apagar Visor)** | ❌ Não | ✅ **Sim (Desliga o display instantaneamente)** |
| **Teste Manual do Display Físico** | ❌ Não | ✅ **Sim (Slider de 0 a 99 em tempo real)** |
| **Alerta Visual Térmico no Gabinete**| ❌ Não | ✅ **Sim (Pisca o display físico se > 85°C)** |
| **Ícone Dinâmico na Bandeja (Tray)**| ❌ Apenas ícone estático | ✅ **Sim (Dígitos ao vivo com cores térmicas adaptativas)** |
| **Diagnóstico USB Embutido** | ❌ Não | ✅ **Sim (Scanner HID e relatório em 1 clique)** |
| **Idioma** | ⚠️ Chinês / Tradução precária | ✅ **Bilingual instantâneo (🇧🇷 PT / 🇺🇸 EN)** |
| **Consumo de Memória RAM** | ⚠️ > 110 MB (3 processos + serviço) | Processo principal + auxiliar de GPU; consumo varia com o hardware |
| **Dependências Externas** | ⚠️ Instalação pesada | **DLLs gerenciadas embutidas; requer .NET Framework 4.8 e PawnIO para CPU** |
| **Tamanho do Executável** | ⚠️ ~30 MB a 50 MB de arquivos | **~1,4 MB (EXE com dependências gerenciadas embutidas)** |
| **Poluição na Área de Trabalho** | ⚠️ Cria atalhos desnecessários | 🚀 **Zero atalhos (opera discretamente na bandeja)** |

---

## ⚙️ Como o Vizzor e o PawnIO Funcionam Juntos?

Para que qualquer programa consiga ler a temperatura da CPU diretamente do processador (em modo privilegiado de kernel *Ring 0*), o Windows exige um driver assinado digitalmente:

* **O que é o `PawnIO` (`PawnIO.sys`):** É o driver oficial de hardware responsável pelo acesso aos registradores de monitoramento térmico (como o microcontrolador **SMU** nos processadores **AMD Ryzen** e registradores **MSR** na linha **Intel**).
* **Como eles trabalham em conjunto:**
  1. O driver **PawnIO** disponibiliza os dados térmicos de baixo nível do processador com segurança e estabilidade;
  2. O **Vizzor** lê esses sensores através da LibreHardwareMonitor;
  3. O motor de protocolo do Vizzor empacota o frame USB HID correspondente e o transmite diretamente ao water cooler a cada segundo.
* **A leitura da CPU exige o driver e os privilégios adequados.** O pacote inclui o instalador do PawnIO e configura a inicialização elevada. Dependências ausentes ou hardware não suportado também podem impedir a leitura; o painel mostra `N/D` quando não há temperatura válida.

---

## 📥 Formas de Baixar e Usar

Escolha a modalidade que melhor atende à sua necessidade:

### Opção 1: Executável Portátil Avulso (.exe) — Mais Fácil e Direto ⭐
*O aplicativo é portátil; a leitura da CPU requer o driver PawnIO instalado:*
1. Baixe o [**Vizzor.exe**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor.exe);
2. Salve em qualquer pasta (Área de Trabalho, Downloads, Pendrive);
3. Dê dois cliques e aceite a elevação do Windows. As dependências gerenciadas já estão embutidas; para os sensores da CPU, o PawnIO precisa estar instalado.

### Opção 2: Pacote Completo com Instalador Automático (.zip)
*Ideal para configurar inicialização automática com o Windows e incluir o instalador do PawnIO:*
1. Baixe o [**Vizzor.zip**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor.zip);
2. Extraia em qualquer pasta;
3. Clique com o botão direito em **`Instalar.bat`** e selecione **Executar como Administrador**;
4. O script verifica o PawnIO, copia os arquivos para `C:\Vizzor`, registra o início com o Windows e inicia o Vizzor na bandeja do sistema ao lado do relógio.

### Opção 3: Modo Portátil Compactado (.zip)
1. Baixe o [**Vizzor.zip**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor.zip);
2. Extraia em qualquer pasta;
3. Dê dois cliques em `Vizzor.exe`. Ele rodará diretamente da pasta atual sem mexer nas pastas do sistema.

### Opção 4: Compilação a partir do Código-Fonte (Executável Único com DLL Embutida)
O único projeto é `src/Vizzor/Vizzor.csproj`, direcionado ao .NET Framework 4.8. Com o SDK .NET instalado, execute no Windows:

```powershell
git clone https://github.com/thegodKoDDa/vizzor.git
cd vizzor
powershell -NoProfile -ExecutionPolicy Bypass -File tools/build_release.ps1
```

O build atualiza `dist` e remove seus temporários. O executável inclui LibreHardwareMonitorLib, HidSharp, RAMSPDToolkit-NDD e System.CodeDom. O PawnIO é um driver separado e precisa estar instalado para os sensores de CPU.

O ZIP só é gerado quando solicitado com `-Archive`, substituindo o arquivo único `dist.zip`. Não são criadas pastas de releases antigos.

Para executar os testes:

```powershell
dotnet build tests/RegressionTests.csproj -c Release
.\tests\bin\Release\net48\RegressionTests.exe
python tests/run_tests.py
```

---

## 🖥️ Recursos e Interface

```text
+--------------------------------------------------------------------+
| 🧊 VIZZOR - Hardware Display & PC Monitor          [🇧🇷 PT / 🇺🇸 EN] |
| Dispositivo: [Modelo do Cooler] ou 🖥️ Modo Standalone (PC Telemetry)|
+--------------------------------------------------------------------+
|   [ REPLICA VISOR ]    |  TELEMETRIA EXPANDIDA EM TEMPO REAL       |
|       ( 48°C )         |  • CPU: Ryzen 7 7800X3D - 48.0°C (18%)    |
|    Mostrador Digital   |    Clock: 4.85 GHz | 72 W | 1.25 V        |
|                        |  • GPU: RTX 4070 SUPER - 44.0°C (5%)      |
|                        |    Hotspot: 52°C | VRAM: 6.2/12.0 GB | 185W |
|                        |  • RAM: 14.8 / 32.0 GB (46%)              |
|                        |  • SSD: Samsung 980 PRO - 38.0°C          |
|                        |  • Placa-Mãe & Fans: 33°C | Fan: 1100 RPM |
+--------------------------------------------------------------------+
| MODOS DE EXIBIÇÃO NO VISOR FÍSICO / BANDEJA:                       |
| (•) CPU Temp   ( ) GPU Temp   ( ) SSD NVMe   ( ) Mais Quente (Hot) |
| ( ) Carga CPU  ( ) Carga RAM  ( ) Alternado  ( ) Relógio (Minutos) |
+--------------------------------------------------------------------+
| [ ] 🎛️ Testar Valor Manual no Visor (0 a 99)                        |
|     [=====================O=============] -> Forçar no display      |
+--------------------------------------------------------------------+
| [🔍 Diagnóstico USB...]                       [Minimizar para Tray]|
+--------------------------------------------------------------------+
```

### 1. Barra de Tarefas do Windows (Bandeja do Sistema / Tray)
O Vizzor vive discretamente na barra inferior do Windows, na área de notificações ao lado do relógio do sistema:
* **Ícone com Números ao Vivo:** Desenha o valor da temperatura em tempo real diretamente no ícone com cores térmicas adaptativas:
  * 🔵 **Ciano Claro:** Operação Normal (< 65 °C)
  * 🟠 **Laranja:** Carga Moderada / Jogos (65 °C a 80 °C)
  * 🔴 **Vermelho:** Alerta Térmico (> 80 °C)
* **Clique com o Botão Direito no Ícone (Menu Contextual Rápido):**
  * **Abrir Painel de Controle...** (ou simplesmente 2 cliques rápidos com botão esquerdo)
  * **Cooler Conectado:** Exibe o modelo específico do seu cooler (se não detectado ou desconectado, o campo fica vazio)
  * **Modo de Exibição:** Troca instantânea entre CPU Temp, GPU Temp, SSD NVMe, O Mais Quente, Alternado (5s), Relógio, Carga CPU (%), Carga RAM (%) e Modo Stealth (Apagar Visor)
  * **Velocidade de Atualização:** Rápido (500 ms), Normal (1s) ou Econômico (2s)
  * **Tempo de Alternância:** Alternar a cada 3s, 5s ou 10s
  * **Efeitos e Alertas:** Ativação do alerta no gabinete (> 85°C), notificações do Windows e Inicialização com o Windows
  * **Desativar Softwares Conflitantes de Fabricantes:** Fecha e desativa programas e serviços de coolers originais em segundo plano (como *tifSer*, *SendTemp*, utilitários da Rise Mode ou DeepCool) para garantir comunicação exclusiva e sem conflitos no barramento USB
  * **Sair do Vizzor**

### 2. Painel de Controle Dark Mode e Modo Standalone (PC Telemetry)
* **Funciona com ou sem Water Cooler:** Mesmo que você não possua um cooler com visor digital USB, o Vizzor atua como um **monitor de hardware de sistema completo e ultraleve**, com a mesma interface e os mesmos sensores disponíveis no modo com cooler.
* **Identificação Automática do Modelo:** Se um cooler suportado estiver conectado, o cabeçalho exibe o modelo correspondente. Se não houver cooler USB, ativa elegantemente o **`🖥️ Monitor de Hardware do Sistema (Modo Standalone)`**.
* **Telemetria Expandida em Tempo Real:**
  * **CPU:** Modelo exato, Temperatura (°C), Carga (%), Clock dinâmico (GHz), Consumo em Watts (W) e Voltagem VCore (V);
  * **GPU:** Modelo da placa dedicada (NVIDIA / AMD), Temperatura Core & Hotspot (°C), Carga (%), VRAM Dedicada (ex: `6.2 / 12.0 GB`), Consumo (W) e Ventoinhas (RPM);
  * **Memória RAM:** Uso detalhado em Gigabytes (ex: `15.4 / 32.0 GB`) e percentual de carga via chamada nativa Win32;
  * **Armazenamento:** Nome do SSD NVMe / SATA e temperatura individual;
  * **Placa-Mãe & Ventoinhas:** Nome da motherboard, temperatura do chipset/sistema e rotação das ventoinhas (CPU Fan RPM).
* **Réplica Gráfica Adaptativa:** Mostrador visual circular no centro com leitura adaptativa da métrica selecionada.

### 3. Slider de Teste Manual Interativo
* Permite sobrepor a leitura dos sensores e testar valores arbitrários de **0 a 99** diretamente no display físico do cooler;
* Essencial para verificar se todos os segmentos de LED do visor estão funcionando perfeitamente.

### 4. Alerta Visual no Gabinete
Se qualquer componente monitorado ultrapassar **85 °C**, o display físico pisca continuamente em ciclo de pulso, permitindo perceber instantaneamente anomalias térmicas pela lateral de vidro do gabinete.

---

## ❓ Perguntas Frequentes & Solução de Problemas

#### A temperatura da CPU está indisponível ou em 0 °C no visor. O que fazer?
O painel mostra `N/D` quando não há leitura válida. Verifique o driver, os privilégios e o log:
1. Certifique-se de executar o `Instalar.bat` como **Administrador** para instalar o `PawnIO`;
2. Se estiver rodando o `.exe` avulso, instale o PawnIO uma única vez e aceite a elevação do Windows;
3. Consulte `%LOCALAPPDATA%\Vizzor\vizzor.log` para identificar falhas de dependências ou sensores. O log tem tamanho limitado a aproximadamente 1 MB.

#### O programa aparece em modo standalone mesmo com cooler?
É o modo automático quando nenhum dispositivo está conectado ao aplicativo. A busca USB agora se repete a cada cinco segundos; use o diagnóstico USB do painel para verificar o VID/PID e a conexão.

#### Uma falha NVIDIA pode derrubar o visor?
A leitura da GPU roda em processo separado. Se esse processo falhar, CPU e USB continuam ativos. Após três falhas, reinicie o Vizzor para tentar a leitura da GPU novamente.

#### O Vizzor cria atalhos na Área de Trabalho?
**Não.** O Vizzor foi projetado para manter a sua Área de Trabalho 100% limpa. Ele opera diretamente a partir da bandeja do sistema (perto do relógio do Windows), exatamente como deve ser um software de monitoramento de hardware.

#### Como desinstalar?
Basta clicar duas vezes em **`Desinstalar.bat`** (como Administrador). Ele fecha o aplicativo, desregistra a inicialização automática e remove a pasta `C:\Vizzor` de forma limpa.

---

## 🌍 English Quick Overview

**Vizzor** is an open-source, ultra-lightweight hardware monitor and USB HID display controller designed to replace bulky vendor utilities for AIO water coolers and digital coolers (such as the *Rise Mode Aura Ice 360*, *DeepCool Digital*, *NZXT Kraken*, *Lian Li Galahad II LCD*, and *Cooler Master Atmos*).

* **Fault isolation:** GPU telemetry runs in a separate worker so native GPU driver crashes do not stop CPU monitoring or USB updates.
* **Binary Size:** About 1.4 MB with managed dependencies embedded; .NET Framework 4.8 and the PawnIO CPU driver are required.
* **Sensors Supported:** CPU (AMD Ryzen & Intel), GPU (NVIDIA & AMD), System RAM, and NVMe M.2 SSDs.
* **Multi-Cooler Engine:** Automatically detects supported USB HID coolers and retries every five seconds while disconnected. Rise Mode Aura Ice has been validated on physical hardware.
* **Interactive Features:** Manual slider display test (0-99), built-in USB HID diagnostics scanner with one-click issue reporting, dynamic taskbar tray numbers with adaptive thermal colors, and high-temp glass alerts (> 85 °C).
* **Bilingual UI:** Click the language button in the dashboard to toggle between English and Portuguese.

---

## 🔬 Documentação Técnica

Para desenvolvedores e entusiastas de engenharia reversa:
* [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — Arquitetura, processos, inicialização e integração de kernel;
* [`docs/PROTOCOL.md`](docs/PROTOCOL.md) — Especificações de pacotes USB HID de cada fabricante decodificado;

---

## 📄 Licença

Distribuído sob a licença de código aberto **MIT**. Consulte o arquivo [`LICENSE`](LICENSE) para mais detalhes.  
Desenvolvido pela comunidade aberta por [thegodKoDDa](https://github.com/thegodKoDDa).
