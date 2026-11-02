# 🧊 Vizzor

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)]()
[![Hardware](https://img.shields.io/badge/Hardware-Rise%20Mode%20Aura%20Ice-00d4ff.svg)]()
[![Size](https://img.shields.io/badge/Size-50%20KB-success.svg)]()

> Software alternativo, leve e de código aberto para o visor LCD do water cooler **Rise Mode Aura Ice Black ARGB 360 mm** (`RM-WAIB-06-ARGB`).

O software original fornecido pelo fabricante é notório por travamentos constantes, consumo excessivo de memória RAM (> 110 MB com múltiplos processos e serviços em segundo plano) e a limitação de exibir exclusivamente a temperatura da CPU. O **Vizzor** foi desenvolvido via engenharia reversa do protocolo USB HID para oferecer uma experiência estável, rica em recursos, com suporte a **GPU (NVIDIA & AMD)** e consumo inferior a 45 MB de RAM.

---

## ⚡ Comparativo: Software Oficial vs Vizzor

| Recurso | Software Oficial (Rise Mode) | Vizzor v3.0 |
| :--- | :---: | :---: |
| **Exibição de Temperatura da GPU** | ❌ Não suportado | ✅ **Sim (NVIDIA GeForce & AMD Radeon)** |
| **Modo Hotspot (O Mais Quente)** | ❌ Não | ✅ **Sim (Compara CPU e GPU dinamicamente)** |
| **Alternância Automática** | ❌ Não | ✅ **Sim (a cada 3s, 5s ou 10s)** |
| **Modo Relógio de Gabinete** | ❌ Não | ✅ **Sim (Minutos no display)** |
| **Modo Stealth (Apagar Visor)** | ❌ Não | ✅ **Sim (Via menu ou painel)** |
| **Animação de Boot (Corrida)** | ❌ Não | ✅ **Sim (Ao inicializar)** |
| **Alerta Visual Térmico no Visor**| ❌ Não | ✅ **Sim (Pisca visor se > 85°C)** |
| **Ícone Dinâmico na Barra de Tarefas**| ❌ Não (ícone estático) | ✅ **Sim (Temperatura ao vivo com cores térmicas)** |
| **Painel de Controle Visual** | ❌ Janela legada e pesada | ✅ **Dashboard Dark Mode moderno** |
| **Consumo de Memória RAM** | ⚠️ ~110 MB (3 processos + serviço) | 🚀 **~40 MB (Processo único e leve)** |
| **Tamanho do Executável** | ⚠️ ~30 MB de arquivos | 🚀 **50 KB (.NET Framework nativo)** |

---

## ⚙️ Por que o PawnIO vem junto e como eles funcionam?

Para ler sensores de temperatura em baixo nível (modo *Ring 0* do processador), o sistema operacional Windows exige um driver de dispositivo com assinatura válida de kernel:

* **O que é o `PawnIO` (`PawnIO.sys`):** É o driver de hardware responsável por intermediar a leitura segura dos registradores internos do processador (como o microcontrolador SMU nos processadores **AMD Ryzen** e registradores MSR na **Intel**).
* **Como eles trabalham juntos:**
  1. O driver **PawnIO** se comunica com a camada de hardware do processador e disponibiliza os dados térmicos;
  2. O **Vizzor** consulta esses sensores com alta velocidade, formata o relatório USB HID de 11 bytes e o envia diretamente para o chip do display a cada segundo.
* **Sem o PawnIO**, o Windows bloqueia o acesso aos registradores de temperatura do processador e o mostrador do cooler ficaria marcando 0 graus. Por isso, incluímos o `PawnIO_setup.exe` no pacote completo. O instalador verifica se você já o possui e, se necessário, realiza a instalação rápida.

---

## 📥 Formas de Baixar e Usar

Escolha a forma que melhor se adapta à sua preferência:

### 1. Pacote Completo com Instalador Automático (Recomendado)
Para quem deseja uma instalação limpa, sem atalhos indesejados e com inicialização automática:
1. Baixe o [**Vizzor_v3.0_Completo.zip**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor_v3.0_Completo.zip);
2. Extraia o conteúdo em qualquer pasta;
3. Execute **`Instalar.bat`** como Administrador;
4. O instalador verifica o PawnIO, copia os arquivos para `C:\Vizzor`, ativa a inicialização com o Windows e inicia o aplicativo diretamente na bandeja do sistema (perto do relógio).

### 2. Executável Portátil Único (.exe)
Para quem prefere não rodar scripts e quer apenas o executável direto:
1. Baixe diretamente o [**Vizzor_Standalone.exe**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor_Standalone.exe) e a biblioteca `LibreHardwareMonitorLib.dll`;
2. Coloque-os na mesma pasta e dê dois cliques no executável;
3. O Vizzor roda imediatamente em modo portátil a partir de qualquer pasta.

### 3. Modo Portátil Compactado (.zip)
1. Baixe o [**Vizzor_v3.0_Completo.zip**](https://github.com/thegodKoDDa/vizzor/releases/latest/download/Vizzor_v3.0_Completo.zip);
2. Extraia na pasta de sua escolha;
3. Dê dois cliques em `Vizzor.exe`.

### 4. A partir do Código-Fonte (Compilação Local)
Para desenvolvedores que desejam inspecionar e compilar por conta própria:
```cmd
git clone https://github.com/thegodKoDDa/vizzor.git
cd vizzor\src\Vizzor_NetFx
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:Vizzor.exe /r:..\..\dist\Vizzor_v3.0\LibreHardwareMonitorLib.dll /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll *.cs
```

---

## 🖥️ Recursos do Aplicativo

### Painel de Controle (Dashboard Dark Mode)
Dê **dois cliques** no ícone do Vizzor na barra de tarefas (ou clique com o botão direito e selecione *"Abrir Painel de Controle..."*):
* Réplica visual estilizada do cabeçote do water cooler refletindo a temperatura em tempo real;
* Telemetria ao vivo de CPU, GPU e uso de Memória RAM;
* Troca instantânea de modos de exibição pelo painel.

### Ícone Dinâmico na Barra de Tarefas
O ícone do Vizzor desenha o número da temperatura ao vivo ao lado do relógio do Windows com cores térmicas adaptativas:
* 🔵 **Ciano:** Normal (< 65 °C)
* 🟠 **Laranja:** Carga Moderada / Jogos (65 °C – 80 °C)
* 🔴 **Vermelho:** Alerta Térmico (> 80 °C)

### Efeitos Visuais no Water Cooler
* **Sequência de Boot:** Contagem rápida de corrida (`00 -> 99`) ao iniciar o aplicativo;
* **Alerta Visual no Gabinete:** O mostrador físico pisca se a temperatura passar de 85 °C para alertar pela lateral de vidro do gabinete;
* **Modo Stealth:** Permite apagar 100% o visor do cooler para dormir ou assistir filmes.

---

## 🔬 Protocolo USB HID Decodificado

* **Vendor ID (VID):** `0xAA88` (`43656`)
* **Product ID (PID):** `0x8666` (`34406`)
* **Dispositivo USB:** `温度显示HID设备` (Fabricante: `铭研科技` - Mingyan Technology)
* **Estrutura do Frame HID (11 bytes):**

```text
Byte 0:  0x00 (Report ID)
Byte 1:  Valor Exibido no Visor (0 a 99) -> Temperatura, Carga ou Minuto
Byte 2:  Temperatura da GPU (°C)
Byte 3:  Uso da CPU (%)
Byte 4:  Uso de Memória RAM (%)
Byte 5:  Temperatura da Placa-Mãe (°C)
Byte 6:  Uso da GPU (%)
Bytes 7-10: Reservado (0x00)
```

Mais detalhes técnicos em [`docs/PROTOCOL.md`](docs/PROTOCOL.md).

---

## 📄 Licença

Distribuído sob a licença **MIT**. Veja [`LICENSE`](LICENSE) para mais detalhes.
