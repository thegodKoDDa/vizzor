# Roadmap e Histórico de Desenvolvimento - Vizzor

Este documento registra os marcos concluídos e os próximos objetivos de evolução do projeto **Vizzor**.

---

## 🏁 Marcos Concluídos

### Marco 1: Engenharia Reversa e Protocolo USB HID ✅
- [x] Descompilação e análise estática dos binários originais (`SendTemp.exe`, `CPU Server.exe`, `Rise Mode Temp CPU Driver R2.2.exe`).
- [x] Mapeamento dos descritores HID (`VID: 0xAA88`, `PID: 0x8666`) e estrutura de payload de 11 bytes.
- [x] Documentação técnica completa em [`docs/PROTOCOL.md`](PROTOCOL.md).

### Marco 2: Prova de Conceito em Hardware Real ✅
- [x] Envio de pacotes de teste controlados diretamente para o cooler físico via HID API.
- [x] Descoberta e validação do mecanismo de Watchdog do microcontrolador (desliga display se sem pacotes por ~2-3s).
- [x] Confirmação das capacidades físicas do display numérico (0 a 99).

### Marco 3: Aplicativo Nativo Vizzor v1.0 / v2.0 ✅
- [x] Migração de arquitetura pesada com 3 processos para um executável único ultraleve em C# .NET.
- [x] Implementação de monitoramento de CPU e GPU via `LibreHardwareMonitorLib`.
- [x] Criação de ícone dinâmico na bandeja do sistema com números ao vivo e cores térmicas (Ciano, Laranja, Vermelho).
- [x] Modos de exibição: CPU Temp, GPU Temp, O Mais Quente (Hotspot), Alternado (5s) e Relógio.
- [x] Alerta visual no gabinete se ultrapassar 85°C.

### Marco 4: Estabilidade de Kernel e Arquitetura .NET Framework 4.8 ✅
- [x] Integração Ring 0 com o driver de kernel `PawnIO.sys` para suporte nativo aos microcontroladores AMD Ryzen SMU e Intel MSR.
- [x] Compilação nativa direcionada ao .NET Framework 4.8 (tamanho final ~69 KB e inicialização imediata).
- [x] Troca da leitura de RAM para chamada Win32 `GlobalMemoryStatusEx` (100% livre de falhas de assembly).

### Marco 5: Expansão Multi-Cooler e Recursos Avançados (v3.1) ✅
- [x] **Multi-Cooler Drivers:** Suporte a Rise Mode Aura Ice, Mingyan OEM, DeepCool Digital, NZXT Kraken, Lian Li e Cooler Master.
- [x] **Slider de Teste Manual (0-99):** Controle interativo ao vivo sobre o display físico no Dashboard.
- [x] **Scanner e Diagnóstico USB:** Janela de diagnóstico HID com botão de cópia formatada para o GitHub.
- [x] **Monitoramento SSD NVMe M.2:** Card de temperatura e modo de exibição no visor.
- [x] **Interface Bilíngue:** Suporte completo em tempo real para Português (pt-BR) e Inglês (en-US).

### Marco 6: Detecção Dinâmica de Modelo e Controle Completo na Tray (v3.2) ✅
- [x] **Detecção Estrita do Modelo:** Exibição dinâmica exclusiva do modelo real conectado (campo vazio se não conectado/detectado).
- [x] **Controle Total pelo Botão Direito:** Menu contextual completo ao clicar na bandeja com alternância de modos, velocidades, alertas e efeitos.
- [x] **Desativação Genérica de Conflitos OEM:** Opção para encerrar e desativar processos concorrentes de múltiplos fabricantes de coolers.
- [x] **Repositório e Release Polidos:** Metadados, About, tópicos e releases organizados e atualizados.

---

## 🚀 Próximos Passos (Futuro)

- [ ] Suporte a curvas de fans / bomba integradas via controle de PWM (para coolers que suportam controle por software).
- [ ] Renderização de imagens customizadas / GIFs (para os modelos de cooler com tela LCD gráfica, como NZXT Elite e Lian Li).
- [ ] Integração com HWiNFO via memória compartilhada (*Shared Memory*).
- [ ] Tradução para Espanhol (es-ES).
