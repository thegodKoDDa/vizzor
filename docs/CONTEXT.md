# Contexto e evidências

## Equipamento e relato do usuário

- Rise Mode Aura Ice Black ARGB 360 mm, RM-WAIB-06-ARGB.
- O visor exibe temperatura. O usuário informa que o programa apenas faz o visor funcionar, sem uma interface útil de personalização.
- O usuário confirma que esse software só mexe no visor.
- Interesse: aplicativo alternativo, possivelmente para outras métricas ou imagens, conforme capacidade real do hardware.

## Instalação examinada no Windows

Origem: C:\Program Files (x86)\Rise Mode Temp CPU Driver R2.2

- Rise Mode Temp CPU Driver R2.2.exe: executável principal .NET; versão interna 0.9.1.0; sem assinatura Authenticode.
- LibreHardwareMonitorLib.dll: versão 0.9.5.0, usada para leitura de sensores.
- CPU Server.exe: registrado como serviço tifSer, início automático, conta LocalSystem. Estava em execução na inspeção.
- SendTemp.exe: estava em execução como filho de CPU Server.exe. Contém referências a USB/HID e NamedPipeClientStream.
- O principal contém referências a NamedPipeServerStream e comunicação HID. SendTemp contém a string tifPipe. A direção e o formato exatos das mensagens não foram decodificados.
- PawnIO: driver observado ativo em C:\Program Files\PawnIO\PawnIO.sys; instalador 2.0.1.0 incluído na pasta.
- Bibliotecas adicionais: HidSharp, OxyPlot, Newtonsoft.Json, RAMSPDToolkit, TaskScheduler e Aga.Controls.
- Configuração principal: aproximadamente 11,7 MB, 747 entradas; inclui séries históricas /values codificadas em Base64 com prefixo compatível com GZip. Não foi decodificada nesta etapa.
- ini.txt estava vazio.
- Há código de servidor HTTP no principal, mas não foi comprovado que esteja habilitado.

## Assinaturas

PawnIO_setup.exe e System.CodeDom.dll retornaram assinatura válida. Diversos componentes retornaram UnknownError com certificado de Keroro Software LLC. Isso não prova validade nem malícia. A inspeção não equivale a uma auditoria de segurança.

## Limites do que sabemos

Ainda desconhecidos: VID/PID do visor, endpoints, tamanho dos relatórios HID, bytes enviados, intervalo de atualização, formato do payload, firmware, tipo físico do display e capacidade gráfica.

Não tratar a hipótese de uma tela gráfica como fato. Pode ser visor de segmentos fixos, tela com comandos predefinidos ou tela gráfica. O nome SendTemp é um indício, não uma especificação do protocolo.

## Fontes web consultadas

- https://www.kabum.com.br/produto/495961/water-cooler-rise-mode-aura-ice-black-argb-360mm-amd-intel-preto-rm-waib-06-argb
- https://www.kabum.com.br/produto/897328/water-cooler-rise-mode-aura-ice-black-argb-360mm-amd-intel-preto-rm-waib-06-argb
- https://risemode.com/en/p/water-cooler-aura-ice-black-360mm-argb
- https://lojarisemode.com.br/p/water-cooler-aura-ice-black-360mm-argb
- URL histórica de manual: https://risemode.com/wp-content/uploads/2024/06/water-cooler-install-manual-Aura-Ice-1.pdf (a abertura retornou HTML, não um PDF válido).

Ficha do anúncio original: 3 ventoinhas 120x120x25 mm; 800–1800 RPM ±10%; 68 CFM; 2,5 mmH2O; 23 dB(A); 12 V; ARGB 5 V; TDP anunciado 300 W.
O outro anúncio do mesmo código diverge: 800–1200 RPM, 60 CFM, vida útil de 40 mil horas contra 80 mil no primeiro. Não há confirmação de revisão de hardware.
O anúncio vendido pela Rise Mode informa CPU_FAN de 4 pinos, AIO_PUMP de 3 pinos, ARGB 3 pinos 5 V e software Aura Ice ARGB Driver para a tela. Não usar números comerciais para inferir desempenho medido.