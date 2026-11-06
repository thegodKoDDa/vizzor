# Protocolos de Comunicação USB HID - Vizzor Multi-Cooler

Este documento descreve as especificações técnicas, identificadores de hardware (VID/PID) e a estrutura dos pacotes USB HID implementados nos drivers do **Vizzor**.

---

## 1. Rise Mode Aura Ice 360 / 240 & Mingyan OEM

* **Fabricante:** Mingyan Technology (`铭研科技`)
* **Vendor ID (VID):** `0xAA88` (`43656`)
* **Product ID (PID):**
  - `0x8666` (`34406`) — Rise Mode Aura Ice Black/White ARGB
  - `0x8667` (`34407`) — Coolers OEM Mingyan Digital v1
  - `0x8668` (`34408`) — Coolers OEM Mingyan Digital v2
* **Nome do Produto USB:** `温度显示HID设备`
* **Tamanho do Relatório HID:** 11 bytes (Report ID `0x00` + 10 bytes de payload)
* **Watchdog de Firmware:** O visor desliga se não receber pacotes a cada 2 a 3 segundos.

### Estrutura do Pacote (11 bytes):
| Byte | Tipo | Descrição | Exemplo |
| :---: | :---: | :--- | :--- |
| **0** | `byte` | HID Report ID (sempre `0x00`) | `0x00` |
| **1** | `byte` | Valor no Mostrador Físico (0 a 99) | `0x2E` (46 °C / 46%) |
| **2** | `byte` | Temperatura da GPU (°C, 0 a 99) | `0x2B` (43 °C) |
| **3** | `byte` | Uso da CPU (%, 0 a 99) | `0x14` (20%) |
| **4** | `byte` | Uso da Memória RAM (%, 0 a 99) | `0x32` (50%) |
| **5** | `byte` | Temperatura da Placa-Mãe (°C, 0 a 99) | `0x22` (34 °C) |
| **6** | `byte` | Uso da GPU (%, 0 a 99) | `0x0F` (15%) |
| **7..10**| `byte` | Reservado / Padding | `0x00` |

---

## 2. DeepCool Digital Series

* **Modelos:** AK400 Digital, AK500 Digital, AK620 Digital, LS720 Digital, CH560 Digital
* **Vendor ID (VID):** `0x3633` (`13875`)
* **Product ID (PID):** `0x0001`, `0x0002`, `0x0003`
* **Tamanho do Relatório HID:** 64 bytes
* **Taxa de Envio:** 1000 ms

### Estrutura do Pacote (64 bytes):
| Offset | Valor | Descrição |
| :---: | :---: | :--- |
| **0** | `0x00` | Report ID |
| **1** | `0xDA` | Cabeçalho do Protocolo DeepCool |
| **2** | `displayVal` | Valor numérico inteiro a ser exibido no display (0 a 99) |
| **3** | `cpuLoad` | Barra de carga da CPU (0 a 100) |
| **4** | `0x01` | Flag de modo Celsius / Ativo |
| **5..63** | `0x00` | Padding com zeros |

---

## 3. NZXT Kraken Series

* **Modelos:** Kraken Z53 / Z63 / Z73, Kraken Elite 240 / 280 / 360, Kraken 2023
* **Vendor ID (VID):** `0x1E71` (`7793`)
* **Product ID (PID):** `0x2007` (Z Series), `0x3008` (Elite), `0x2001` (Kraken 2023)
* **Tamanho do Relatório HID:** 64 bytes

### Estrutura do Pacote (64 bytes):
| Offset | Valor | Descrição |
| :---: | :---: | :--- |
| **0** | `0x00` | Report ID |
| **1** | `0x70` | Header de controle de telemetria |
| **2** | `0x01` | Sub-comando de exibição |
| **3** | `displayVal` | Valor numérico principal (0 a 99) |
| **4** | `gpuTemp` | Temperatura da GPU (0 a 100) |
| **5..63** | `0x00` | Padding com zeros |

---

## 4. Lian Li Series

* **Modelos:** Galahad II LCD, UNI FAN TL LCD
* **Vendor ID (VID):** `0x0CF2` (`3314`)
* **Product ID (PID):** `0xA102`, `0xA100`, `0xA101`
* **Tamanho do Relatório HID:** 64 bytes

### Estrutura do Pacote (64 bytes):
| Offset | Valor | Descrição |
| :---: | :---: | :--- |
| **0** | `0x00` | Report ID |
| **1** | `0xE0` | Cabeçalho Lian Li LCD |
| **2** | `displayVal` | Valor numérico principal (0 a 99) |
| **3** | `gpuTemp` | Temperatura da GPU (0 a 100) |
| **4..63** | `0x00` | Padding com zeros |

---

## 5. Cooler Master Series

* **Modelos:** MasterLiquid 360 Atmos / LCD Series
* **Vendor ID (VID):** `0x2516` (`9494`)
* **Product ID (PID):** `0x1001`, `0x1002`, `0x0100`
* **Tamanho do Relatório HID:** 64 bytes

### Estrutura do Pacote (64 bytes):
| Offset | Valor | Descrição |
| :---: | :---: | :--- |
| **0** | `0x00` | Report ID |
| **1** | `0x55` | Cabeçalho Cooler Master |
| **2** | `displayVal` | Valor numérico principal (0 a 99) |
| **3** | `gpuTemp` | Temperatura da GPU (0 a 100) |
| **4..63** | `0x00` | Padding com zeros |
