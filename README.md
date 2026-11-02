# 🧊 Vizzor

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)]()
[![Release](https://img.shields.io/badge/Release-v3.1-success.svg)](https://github.com/thegodKoDDa/vizzor/releases/latest)
[![Size](https://img.shields.io/badge/Size-~69%20KB-brightgreen.svg)]()
[![Language](https://img.shields.io/badge/i18n-PT--BR%20%7C%20EN--US-orange.svg)]()

> **Software aberto, independente e ultraleve para displays e visores de water coolers e coolers digitais.**  
> *Open-source, ultra-lightweight hardware monitor & display controller for AIO water coolers and digital air coolers.*

O **Vizzor** foi criado para substituir os softwares proprietários fornecidos pelos fabricantes (como o da Rise Mode e outros utilitários pesados), que frequentemente sofrem de travamentos constantes, consomem mais de 110 MB de RAM em múltiplos processos e limitam o visor a exibir apenas a temperatura da CPU.

Construído via engenharia reversa direta sobre o protocolo **USB HID**, o Vizzor opera em um **único processo nativo**, consome **menos de 45 MB de RAM**, suporta múltiplos fabricantes e permite monitorar **CPU, GPU (NVIDIA & AMD), Memória RAM e SSD NVMe M.2**.

---

## 🧊 Hardware Suportado (Multi-Cooler Engine)

O Vizzor possui um motor de **Detecção Automática (Auto-Detect)** que escaneia o barramento USB do computador ao iniciar e conecta o driver apropriado:

| Fabricante / Marca | Modelos Suportados | Identificação USB | Status |
| :--- | :--- | :---: | :---: |
| **Rise Mode** | Aura Ice Black & White ARGB 240 mm / 360 mm (`RM-WAIB-06-ARGB`) | `VID: 0xAA88`, `PID: 0x8666` | ✅ Validado no Hardware Real |
| **SuperFrame / Alseye** | Coolers Digitais OEM Mingyan | `VID: 0xAA88`, `PID: 0x8667`, `0x8668` | ✅ Suportado |
| **DeepCool** | AK400 Digital, AK500 Digital, AK620 Digital, CH560 Digital | `VID: 0x3633`, `PID: 0x0001`, `0x0002` | ✅ Suportado |
| **NZXT** | Kraken Z53/Z63/Z73, Kraken Elite 240/280/360, Kraken 2023 | `VID: 0x1E71`, `PID: 0x2007`, `0x3008`, `0x2001` | ✅ Suportado |
| **Lian Li** | Galahad II LCD, UNI FAN TL LCD | `VID: 0x0CF2`, `PID: 0xA102` | ✅ Suportado |
| **Cooler Master** | MasterLiquid 360 Atmos / LCD Series | `VID: 0x2516`, `PID: 0x1001` | ✅ Suportado |

### 🔍 Scanner e Diagnóstico USB Integrado
Tem um water cooler diferente ou não sabe o modelo exato?
1. Abra o painel do Vizzor e clique no botão **`🔍 Diagnóstico USB`**;
2. O sistema listará automaticamente todos os dispositivos USB HID conectados, fabricantes e IDs (`VID` e `PID`);
3. Clique em **`📋 Copiar Relatório`** e cole em uma **[Nova Issue no GitHub](https://github.com/thegodKoDDa/vizzor/issues)** para adicionarmos suporte oficial ao seu cooler!

---

## ⚡ Comparativo: Software Oficial vs Vizzor

| Recurso | Softwares Proprietários Legados | Vizzor v3.1 |
| :--- | :---: | :---: |
| **Temperatura da GPU** | ❌ Não suportado | ✅ **Sim (NVIDIA GeForce & AMD Radeon)** |
| **Temperatura do SSD NVMe M.2** | ❌ Não | ✅ **Sim (Telemetria ao vivo e no display)** |
| **Modo Hotspot (O Mais Quente)** | ❌ Não | ✅ **Sim (Compara CPU e GPU em tempo real)** |
| **Alternância Automática** | ❌ Não | ✅ **Sim (Alterna sensores a cada 5s)** |
| **Modo Relógio no Visor** | ❌ Não | ✅ **Sim (Exibe os minutos atuais)** |
| **Modo Stealth (Apagar Visor)** | ❌ Não | ✅ **Sim (Desliga o display instantaneamente)** |
| **Teste Manual do Display Físico** | ❌ Não | ✅ **Sim (Slider de 0 a 99 em tempo real)** |
| **Alerta Visual Térmico no Gabinete**| ❌ Não | ✅ **Sim (Pisca o display físico se > 85°C)** |
| **Ícone Dinâmico na Bandeja (Tray)**| ❌ Apenas ícone estático | ✅ **Sim (Dígitos ao vivo com cores térmicas adaptativas)** |
| **Diagnóstico USB Embutido** | ❌ Não | ✅ **Sim (Scanner HID e relatório em 1 clique)** |
| **Idioma** | ⚠️ Chinês / Tradução precária | ✅ **Bilingual instantâneo (🇧🇷 PT / 🇺🇸 EN)** |
| **Consumo de Memória RAM** | ⚠️ > 110 MB (3 processos + serviço) | 🚀 **~40 MB (Processo único, ultraleve)** |
| **Tamanho do Executável** | ⚠️ ~30 MB a 50 MB de arquivos | 🚀 **~69 KB (.NET Framework nativo)** |
| **Poluição na Área de Trabalho** | ⚠️ Cria atalhos desnecessários | 🚀 **Zero atalhos (opera discretamente na bandeja)** |

---

## ⚙️ Como o Vizzor e o PawnIO Funcionam Juntos?

Para que qualquer programa consiga ler a temperatura da CPU diretamente do processador (em modo privilegiado de kernel *Ring 0*), o Windows exige um driver assinado digitalmente:

* **O que é o `PawnIO` (`PawnIO.sys`):** É o driver oficial de hardware responsável pelo acesso aos registradores de monitoramento térmico (como o microcontrolador **SMU** nos processadores **AMD Ryzen** e registradores **MSR** na linha **Intel**).
* **Como eles trabalham em conjunto:**
  1. O driver **PawnIO** disponibiliza os dados térmicos de baixo nível do processador com segurança e estabilidade;
  2. O **Vizzor** lê esses sensores em microssegundos através de sua camada nativa;
  3. O motor de protocolo do Vizzor empacota o frame USB HID correspondente e o transmite diretamente ao water cooler a cada segundo.
* **Sem o PawnIO**, o Windows bloqueia o acesso aos registradores de temperatura do processador e qualquer software de monitoramento fica travado marcando **0 °C**. Por essa razão, fornecemos o instalador oficial do PawnIO junto com o Vizzor. O instalador automático detecta se você já possui o PawnIO instalado e só o instala se necessário.

---

## 📥 Formas de Baixar e Usar

Escolha a modalidade que melhor atende à sua necessidade:

### Opção 1: Pacote Completo com Instalador Automático (Recomendado)
*Perfeita para quem quer praticidade, sem atalhos indesejados na área de trabalho e com início automático com o Windows:*
1. Baixe o [**Vizzor_v3.1_Completo.zip**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor_v3.1_Completo.zip);
2. Extraia o arquivo ZIP em qualquer pasta;
3. Clique com o botão direito em **`Instalar.bat`** e selecione **Executar como Administrador**;
4. O script verifica o PawnIO, copia os arquivos para `C:\Vizzor`, registra o início com o Windows e inicia o Vizzor imediatamente na bandeja do sistema ao lado do relógio.

### Opção 2: Executável Portátil Avulso (.exe)
*Ideal para usuários avançados que preferem não executar scripts:*
1. Baixe o [**Vizzor_Standalone.exe**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor_Standalone.exe);
2. Mantenha-o na mesma pasta que a DLL `LibreHardwareMonitorLib.dll` e com o driver PawnIO previamente instalado;
3. Dê dois cliques em `Vizzor_Standalone.exe` para rodar diretamente.

### Opção 3: Modo Portátil Compactado (.zip)
1. Baixe o [**Vizzor_v3.1_Completo.zip**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor_v3.1_Completo.zip);
2. Extraia em qualquer pasta (ex: em um pendrive ou pasta Documentos);
3. Dê dois cliques em `Vizzor.exe`. Ele rodará diretamente da pasta atual sem mexer nas pastas do sistema.

### Opção 4: Compilação a partir do Código-Fonte
O Vizzor utiliza exclusivamente o compilador C# nativo do Windows (`csc.exe`), sem precisar instalar SDKs pesados:
```cmd
git clone https://github.com/thegodKoDDa/vizzor.git
cd vizzor\src\Vizzor_NetFx
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:Vizzor.exe /r:..\..\dist\Vizzor_v3.0\LibreHardwareMonitorLib.dll /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll *.cs
```

---

## 🖥️ Recursos e Interface

```text
+--------------------------------------------------------------+
| 🧊 VIZZOR - Hardware Display Controller           [🇧🇷 PT / 🇺🇸 EN] |
| Dispositivo: Rise Mode Aura Ice 360 (Conectado via USB HID)  |
+--------------------------------------------------------------+
|   [ REPLICA VISOR ]    |  TELEMETRIA AO VIVO                 |
|       ( 46°C )         |  • CPU (Ryzen 5 5600X):  46.4°C (22%)|
|     Aura Ice Head      |  • GPU (RTX 5060 Ti):    42.0°C (12%)|
|                        |  • RAM:                  48.1%       |
|                        |  • SSD (NVMe M.2):       38.0°C      |
+--------------------------------------------------------------+
| MODOS DE EXIBIÇÃO NO VISOR FÍSICO:                           |
| (•) CPU Temp   ( ) GPU Temp   ( ) SSD NVMe   ( ) Mais Quente |
| ( ) Alternado (5s)            ( ) Relógio (Minutos)          |
+--------------------------------------------------------------+
| [ ] 🎛️ Testar Valor Manual no Visor (0 a 99)                  |
|     [=====================O=============] -> Forçar no display|
+--------------------------------------------------------------+
| [🔍 Diagnóstico USB...]                 [Minimizar para Tray]|
+--------------------------------------------------------------+
```

### 1. Painel de Controle Dark Mode
* Dê **dois cliques** no ícone do Vizzor na barra de tarefas (ou clique com botão direito -> *Abrir Painel de Controle...*);
* Réplica gráfica estilizada do cabeçote do water cooler com anel luminoso adaptativo;
* Telemetria completa em tempo real para todos os componentes principais.

### 2. Slider de Teste Manual Interativo
* Permite sobrepor a leitura dos sensores e testar valores arbitrários de **0 a 99** diretamente no display físico do cooler;
* Essencial para verificar se todos os segmentos de LED do visor estão funcionando perfeitamente.

### 3. Cores Térmicas Adaptativas na Barra de Tarefas
O ícone do Vizzor renderiza a temperatura atual com cores visuais dinâmicas:
* 🔵 **Ciano Claro:** Operação Normal (< 65 °C)
* 🟠 **Laranja:** Carga Moderada / Jogos (65 °C a 80 °C)
* 🔴 **Vermelho:** Alerta Térmico (> 80 °C)

### 4. Alerta Visual no Gabinete
Se qualquer componente monitorado ultrapassar **85 °C**, o display físico pisca continuamente em ciclo de pulso, permitindo perceber instantaneamente anomalias térmicas pela lateral de vidro do gabinete.

---

## ❓ Perguntas Frequentes & Solução de Problemas

#### A temperatura da CPU está marcando 0 °C no mostrador. O que fazer?
Isso indica que o driver de kernel Ring 0 não foi instalado ou o Windows não concedeu privilégios de administrador:
1. Certifique-se de executar o `Instalar.bat` como **Administrador** para instalar o `PawnIO`;
2. Se estiver rodando o `.exe` avulso, execute o `PawnIO_setup.exe` uma única vez.

#### O Vizzor cria atalhos na Área de Trabalho?
**Não.** O Vizzor foi projetado para manter a sua Área de Trabalho 100% limpa. Ele opera diretamente a partir da bandeja do sistema (perto do relógio do Windows), exatamente como deve ser um software de monitoramento de hardware.

#### Como desinstalar?
Basta clicar duas vezes em **`Desinstalar.bat`** (como Administrador). Ele fecha o aplicativo, desregistra a inicialização automática e remove a pasta `C:\Vizzor` de forma limpa.

---

## 🌍 English Quick Overview

**Vizzor** is an open-source, ultra-lightweight hardware monitor and USB HID display controller designed to replace bulky vendor utilities for AIO water coolers and digital coolers (such as the *Rise Mode Aura Ice 360*, *DeepCool Digital*, *NZXT Kraken*, *Lian Li Galahad II LCD*, and *Cooler Master Atmos*).

* **Memory Footprint:** Less than 45 MB RAM (compared to 110+ MB with 3 background processes from proprietary apps).
* **Binary Size:** Just ~69 KB native Windows executable.
* **Sensors Supported:** CPU (AMD Ryzen & Intel), GPU (NVIDIA & AMD), System RAM, and NVMe M.2 SSDs.
* **Multi-Cooler Engine:** Automatically detects connected USB HID coolers and binds the corresponding driver.
* **Interactive Features:** Manual slider display test (0-99), built-in USB HID diagnostics scanner with one-click issue reporting, dynamic taskbar tray numbers with adaptive thermal colors, and high-temp glass alerts (> 85 °C).
* **Bilingual UI:** Click the language button in the dashboard to toggle between English and Portuguese.

---

## 🔬 Documentação Técnica

Para desenvolvedores e entusiastas de engenharia reversa:
* [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — Arquitetura de software, modelo de threads e integração de kernel;
* [`docs/PROTOCOL.md`](docs/PROTOCOL.md) — Especificações de pacotes USB HID de cada fabricante decodificado;
* [`docs/ROADMAP.md`](docs/NEXT-STEPS.md) — Histórico de marcos concluídos e planos futuros.

---

## 📄 Licença

Distribuído sob a licença de código aberto **MIT**. Consulte o arquivo [`LICENSE`](LICENSE) para mais detalhes.  
Desenvolvido pela comunidade aberta por [thegodKoDDa](https://github.com/thegodKoDDa).
