# Próximos passos

1. Conferir evidence/manifest.json e trabalhar em cópias dos arquivos originais.
2. Fazer análise estática dos executáveis .NET com um descompilador adequado. Começar por SendTemp.exe (pequeno), depois CPU Server.exe e os métodos HID/pipe do aplicativo principal. Não executar instaladores.
3. Localizar VID/PID, filtros de interface HID, report IDs, tamanho de pacotes, chamadas WriteFile/SetFeature e construção dos bytes. Documentar trechos e interpretação em docs/PROTOCOL.md quando houver evidência.
4. No Windows, identificar o dispositivo USB e coletar descritores sem enviar comandos experimentais. Separar o visor de outros dispositivos HID conectados.
5. Se necessário, observar tráfego normal do aplicativo original. Avaliar ferramentas e eventual instalação de driver de captura antes de alterar o ambiente. Registrar condições e correspondência entre temperatura observada e mensagens.
6. Determinar a capacidade: segmentos, elementos fixos ou framebuffer. Não prometer imagens antes dessa conclusão.
7. Só então desenhar uma prova mínima de envio de um valor conhecido. Evitar dois programas escrevendo no visor simultaneamente e planejar retorno ao original.
8. Escolher a arquitetura do aplicativo após entender o protocolo. Um backend Windows pode ser necessário mesmo com desenvolvimento no WSL. Não assumir que USB passthrough esteja configurado.

## Critério para o primeiro marco

Identidade USB confirmada e formato de pelo menos uma mensagem normal explicado com evidência. A preparação atual ainda não atingiu esse marco.