# Próximos passos

# Próximos passos

- [x] 1. Conferir evidence/manifest.json e trabalhar em cópias dos arquivos originais.
- [x] 2. Fazer análise estática dos executáveis .NET com um descompilador adequado (`SendTemp.exe`, `CPU Server.exe` e `Rise Mode Temp CPU Driver R2.2.exe`).
- [x] 3. Localizar VID/PID, interface HID, report IDs, tamanho de pacotes e construção dos bytes. Documentado em `docs/PROTOCOL.md`.
- [ ] 4. No Windows, identificar o dispositivo USB físico (`VID: 0xAA88`, `PID: 0x8666`) e confirmar descritores.
- [ ] 5. Desenvolver ferramenta de prova de conceito (PoC / CLI de teste em Python ou C#) para envio controlado de valores conhecidos.
- [ ] 6. Testar no hardware real se outras métricas (GPU Temp, CPU%, RAM%) causam efeito ou se o visor físico só exibe o byte de CPU.
- [ ] 7. Desenhar e estruturar a arquitetura do aplicativo definitivo.

## Status do primeiro marco

**Concluído:** Identidade USB confirmada (`VID 0xAA88`, `PID 0x8666`) e formato das mensagens HID de 24 bytes decodificado com evidências extraídas do código-fonte.
