# Vizzor

Investigação e possível software alternativo para o visor do water cooler Rise Mode Aura Ice Black ARGB 360 mm, RM-WAIB-06-ARGB.

## Estado atual

Primeiro marco atingido. O protocolo HID de comunicação com o visor foi decodificado a partir da engenharia reversa dos binários originais. Os identificadores de hardware (`VID: 0xAA88`, `PID: 0x8666`) e o payload de 24 bytes foram identificados e documentados em `docs/PROTOCOL.md`.

O usuário confirma que o software original só controla o visor. Não assumir controle de bomba, ventoinhas ou iluminação.

## Objetivo

Desenvolver um aplicativo/serviço próprio leve e confiável para alimentar o visor com informações do sistema (temperatura de CPU, GPU, porcentagens etc.), sem a necessidade do software proprietário.

## Conteúdo

- docs/CONTEXT.md: fatos, hipóteses e fontes da investigação anterior.
- docs/NEXT-STEPS.md: etapas do plano de desenvolvimento.
- docs/PROTOCOL.md: especificação técnica do protocolo USB HID e mapeamento dos bytes.
- evidence/vendor/: cópia local dos arquivos da instalação original.
- evidence/manifest.json: tamanhos e hashes SHA-256 da cópia.
- evidence/captures/: reservado para capturas de tráfego.
- src/ e tools/: reservados para implementação e ferramentas de teste.

## Ambiente

Pasta Linux: /home/gabriel/dev/vizzor
Acesso Windows: \\wsl.localhost\Ubuntu\home\gabriel\dev\vizzor

Git, Python 3 e dotnet já estão disponíveis no Ubuntu. Não foram instalados pacotes nesta preparação.

O dispositivo está conectado ao Windows. A existência da pasta no WSL não dá acesso automático ao USB. Inicialmente, analisar os arquivos offline no WSL e planejar a observação do software original no Windows.

Os binários e as configurações originais ficam fora do Git. Preserve-os localmente e não publique arquivos de terceiros ou históricos de sensores sem revisar seu conteúdo.