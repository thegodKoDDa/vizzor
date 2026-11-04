# Arquitetura

O projeto `src/Vizzor/Vizzor.csproj` compila o aplicativo Windows em .NET Framework 4.8. O executável embute LibreHardwareMonitorLib, HidSharp, RAMSPDToolkit-NDD e System.CodeDom; o resolver de assemblies é registrado antes da execução das classes dependentes.

## Processos e sensores

O processo principal mantém a interface WinForms, o ícone da bandeja, os sensores de CPU/RAM/armazenamento/placa-mãe e a comunicação USB. O driver PawnIO permite o acesso privilegiado aos sensores da CPU.

A GPU é consultada em outro processo do mesmo executável, com o argumento interno `--gpu-worker`. Amostras são transmitidas por stdout binário redirecionado e expiram após cinco segundos. A saída inesperada desse processo não interrompe o principal. Há até três tentativas de iniciar a leitura da GPU por execução do aplicativo.

Isso contém falhas nativas como AccessViolation dentro da biblioteca NVIDIA, das quais um catch comum não garante recuperação no mesmo processo.

## Temperatura da CPU

A prioridade é Tctl/Tdie, Tdie, Package, Tctl, CCD e demais sensores. CCDs individuais têm prioridade inferior ao die/package. Em empate, usa a maior leitura; valores ausentes, NaN e infinitos são ignorados. A prioridade independe da ordem de enumeração. O painel mostra N/D na ausência de temperatura válida.

## Dispositivos USB

`DeviceManager` mantém os drivers que implementam `IDisplayDevice`. A busca é repetida a cada cinco segundos quando não há conexão ativa. O modo de monitor de PC é ativado quando nenhum cooler suportado está conectado; não existe outro executável para esse modo.

O timer da interface envia os dados ao dispositivo ativo. Falhas de abertura/escrita do Rise Mode são registradas no log. Detalhes dos pacotes ficam em [PROTOCOL.md](PROTOCOL.md).

## Inicialização e instalação

O manifesto solicita privilégios de administrador. A tarefa agendada `Vizzor-<SID>` usa token interativo do usuário, nível mais alto, atraso de 15 segundos após o login e nenhum limite de duração. A chave Run legada é migrada para essa tarefa.

O instalador valida as dependências embutidas, encerra as instâncias do aplicativo e substitui os arquivos em `C:\Vizzor`. Não cria backups. A tarefa é usada também para iniciar o programa ao concluir a instalação.

O build mantém uma única distribuição em `dist`, usa uma pasta temporária do sistema e a remove ao terminar. A geração de ZIP é opcional e substitui um único arquivo `dist.zip`.

## Logs

`AppLog` grava em `%LOCALAPPDATA%\Vizzor\vizzor.log`, limitado a aproximadamente 1 MB. O início da execução registra o caminho do EXE; os primeiros ciclos registram o sensor da CPU, as temperaturas e o resultado do envio USB.
