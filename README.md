# Vizzor

Investigação e possível software alternativo para o visor do water cooler Rise Mode Aura Ice Black ARGB 360 mm, RM-WAIB-06-ARGB.

## Estado atual

**Projeto concluído e em produção.**
O software nativo **Vizzor v2.0** foi desenvolvido, validado e está em execução contínua no Windows do usuário, substituindo com sucesso o software e o serviço proprietários da Rise Mode.

O visor exibe a temperatura real do **AMD Ryzen 5 5600X** e da **NVIDIA GeForce RTX 5060 Ti** com suporte a modos configuráveis e consumo inferior a 50 MB de RAM.

## Funcionalidades do Vizzor v2.0

- **Ícone Dinâmico na Bandeja (Live Tray Icon):** Desenha a temperatura atualizada em tempo real ao lado do relógio do Windows com cores térmicas adaptativas (Ciano < 65°C, Laranja 65–80°C, Vermelho > 80°C).
- **Múltiplos Modos de Exibição:**
  - Temperatura da CPU (Ryzen SMU Tctl/Tdie);
  - Temperatura da GPU (NVIDIA NVAPI Core);
  - O Mais Quente (CPU/GPU Hotspot dinâmico);
  - Alternância periódica (troca automática a cada 3s, 5s ou 10s);
  - Uso da CPU (%) e Uso de Memória RAM (%);
  - Modo Stealth (permite apagar o visor quando desejado).
- **Controle de Frequência de Atualização:** 500 ms (rápido), 1000 ms (normal) ou 2000 ms (econômico).
- **Alerta de Alta Temperatura:** Notificação nativa do Windows se a CPU ou GPU ultrapassar 85°C.
- **Auto-início com o Windows:** Alternável com 1 clique no menu.
- **Desativação Integrada:** Botão no menu para desativar o serviço antigo `tifSer` da Rise Mode.
- **Executável Único:** Apenas 19 KB, sem instalador, com ícone de alta resolução embutido.

## Estrutura do Repositório

- `docs/CONTEXT.md`: histórico e análise dos componentes originais.
- `docs/PROTOCOL.md`: especificação do protocolo USB HID decodificado.
- `docs/NEXT-STEPS.md`: registro dos marcos atingidos.
- `src/Vizzor_NetFx/`: código-fonte da aplicação nativa .NET Framework 4.8.
- `tools/`: ferramentas de teste, verificação e scripts auxiliares.
- `evidence/`: cópia de segurança dos binários originais do fabricante.

## Ambiente

Pasta Linux: /home/gabriel/dev/vizzor
Acesso Windows: \\wsl.localhost\Ubuntu\home\gabriel\dev\vizzor

Git, Python 3 e dotnet já estão disponíveis no Ubuntu. Não foram instalados pacotes nesta preparação.

O dispositivo está conectado ao Windows. A existência da pasta no WSL não dá acesso automático ao USB. Inicialmente, analisar os arquivos offline no WSL e planejar a observação do software original no Windows.

Os binários e as configurações originais ficam fora do Git. Preserve-os localmente e não publique arquivos de terceiros ou históricos de sensores sem revisar seu conteúdo.