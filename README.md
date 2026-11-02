# Vizzor

Investigação e possível software alternativo para o visor do water cooler Rise Mode Aura Ice Black ARGB 360 mm, RM-WAIB-06-ARGB.

## Estado atual

Ambiente preparado para continuidade. Nenhum protocolo foi decodificado, nenhum comando experimental foi enviado ao dispositivo e nenhum software alternativo foi implementado.

O usuário confirma que o software original só controla o visor. Não assumir controle de bomba, ventoinhas ou iluminação.

## Objetivo

Descobrir o que o visor aceita receber e avaliar um aplicativo próprio para exibir outras informações. A possibilidade de imagens, layouts ou animações ainda não está demonstrada.

## Conteúdo

- docs/CONTEXT.md: fatos, hipóteses e fontes da investigação anterior.
- docs/NEXT-STEPS.md: etapas para iniciar a investigação técnica.
- evidence/vendor/: cópia local dos arquivos da instalação original, incluindo configurações e histórico de sensores.
- evidence/manifest.json: tamanhos e hashes SHA-256 da cópia.
- evidence/captures/: reservado para capturas futuras.
- src/ e tools/: reservados para implementação e ferramentas futuras.

## Ambiente

Pasta Linux: /home/gabriel/dev/vizzor
Acesso Windows: \\wsl.localhost\Ubuntu\home\gabriel\dev\vizzor

Git, Python 3 e dotnet já estão disponíveis no Ubuntu. Não foram instalados pacotes nesta preparação.

O dispositivo está conectado ao Windows. A existência da pasta no WSL não dá acesso automático ao USB. Inicialmente, analisar os arquivos offline no WSL e planejar a observação do software original no Windows.

Os binários e as configurações originais ficam fora do Git. Preserve-os localmente e não publique arquivos de terceiros ou históricos de sensores sem revisar seu conteúdo.