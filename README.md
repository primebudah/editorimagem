# AI Image Chat

Aplicativo profissional para Windows especializado em edição, restauração, reconstrução e aprimoramento de imagens utilizando a API da OpenAI.

## 🌟 Características

- **Interface Moderna e Intuitiva**: Design inspirado no ChatGPT Desktop com tema escuro
- **Processamento Inteligente**: Sistema de duas etapas com autoavaliação automática
- **Múltiplas Tentativas**: Configurável para garantir a melhor qualidade
- **Exportação Versátil**: Suporte a PNG, JPG, WEBP, TIFF, PDF e mais
- **Visualizador Avançado**: Zoom, pan, tela cheia e comparação antes/depois
- **Arrastar e Soltar**: Upload fácil de múltiplas imagens
- **Segurança**: API Key armazenada de forma criptografada
- **Sem Login**: Funciona apenas com sua API Key da OpenAI
- **Arquitetura Limpa**: MVVM com separação em camadas

## 📋 Requisitos

- Windows 10 ou superior
- .NET 8.0 Runtime
- Conexão com internet
- API Key da OpenAI

## 🚀 Instalação

### Via Instalador

1. Baixe o instalador `AIImageChat_Setup.exe`
2. Execute o instalador
3. Siga as instruções na tela

### Via Código Fonte

1. Clone este repositório
2. Abra o projeto no Visual Studio 2022
3. Restaure os pacotes NuGet
4. Compile e execute

## ⚙️ Configuração

Ao abrir o aplicativo pela primeira vez:

1. Clique no botão ⚙ no canto superior direito
2. Insira sua API Key da OpenAI
3. Selecione o modelo desejado (gpt-4-vision-preview, gpt-4o, gpt-4o-mini)
4. Configure o número de tentativas automáticas (1-3)
5. Ative a autoavaliação se desejar
6. Escolha a pasta padrão de exportação
7. Clique em "Salvar"

## 💡 Como Usar

### Processar uma Imagem

1. Clique no botão 📎 ou arraste imagens para a janela
2. Digite sua instrução na caixa de texto
3. Exemplos de instruções:
   - "Melhore esta foto"
   - "Remova o fundo"
   - "Troque o céu"
   - "Restaure esta imagem"
   - "Aumente a resolução"
   - "Remova pessoas"
   - "Faça parecer tirada por uma câmera profissional"
   - "Remova riscos"
   - "Corrija iluminação"
   - "Deixe mais nítida"
   - "Transforme em pintura"
   - "Transforme em anime"
   - "Reconstrua áreas danificadas"
4. Clique em "Enviar"
5. Aguarde o processamento
6. A imagem processada aparecerá na conversa

### Visualizar Resultado

1. Clique na imagem resultante
2. Use os controles de zoom (+, -, ⟲)
3. Ative tela cheia com o botão ⛶
4. Exporte em diferentes formatos
5. Copie para área de transferência
6. Abra a pasta do arquivo

### Exportar Imagens

No visualizador, você pode:
- Salvar como PNG
- Salvar como JPG
- Salvar como WEBP
- Salvar como TIFF
- Salvar como PDF
- Copiar imagem
- Salvar em... (formato personalizado)
- Abrir pasta

## 🏗️ Arquitetura

O projeto segue a arquitetura MVVM (Model-View-ViewModel):

```
AIImageChat/
├── Models/              # Modelos de dados
│   ├── Settings.cs
│   ├── ChatMessage.cs
│   └── ImageInfo.cs
├── ViewModels/          # ViewModels
│   ├── ViewModelBase.cs
│   ├── RelayCommand.cs
│   ├── MainViewModel.cs
│   ├── SettingsViewModel.cs
│   └── ImageViewerViewModel.cs
├── Views/               # Views (XAML)
│   ├── MainWindow.xaml
│   ├── SettingsWindow.xaml
│   └── ImageViewerWindow.xaml
├── Services/            # Serviços
│   ├── IOpenAIService.cs
│   ├── OpenAIService.cs
│   ├── IEncryptionService.cs
│   ├── EncryptionService.cs
│   ├── IImageExportService.cs
│   ├── ImageExportService.cs
│   ├── ISettingsService.cs
│   └── SettingsService.cs
└── Converters/          # Conversores XAML
    ├── BoolToVisibilityConverter.cs
    ├── CountToVisibilityConverter.cs
    ├── BoolToWindowStateConverter.cs
    └── ZoomToTransformConverter.cs
```

## 🔒 Segurança

- API Key armazenada localmente de forma criptografada usando AES
- Nenhum dado é compartilhado além do necessário para as chamadas da API
- Configurações salvas em `%LocalAppData%\AIImageChat`
- Sem servidor próprio - comunicação direta com OpenAI

## 🛠️ Tecnologias

- **C#** - Linguagem principal
- **.NET 8** - Framework
- **WPF** - Interface gráfica
- **MVVM** - Padrão de arquitetura
- **SkiaSharp** - Processamento de imagens
- **Newtonsoft.Json** - Serialização JSON
- **Microsoft.Extensions.DependencyInjection** - Injeção de dependência

## 📝 Fluxo Inteligente

O aplicativo utiliza um fluxo de processamento em duas etapas:

1. **Primeira Etapa**: Processamento inicial da imagem com base na instrução do usuário
2. **Segunda Etapa**: Autoavaliação automática que verifica:
   - O pedido foi atendido?
   - Existem artefatos visuais?
   - Existem distorções?
   - Existem bordas defeituosas?
   - A qualidade parece profissional?
3. **Iteração**: Se problemas forem identificados, nova tentativa é gerada automaticamente
4. **Resultado**: A melhor versão encontrada é exibida ao usuário

## ⚠️ Tratamento de Erros

O aplicativo trata os seguintes erros de forma amigável:

- Sem conexão com internet
- API indisponível
- API Key inválida
- Imagem inválida
- Formato incompatível
- Arquivo corrompido
- Timeout
- Limite de uso atingido

## 🔧 Desenvolvimento

### Compilar

```bash
dotnet build
```

### Executar

```bash
dotnet run
```

### Criar Instalador

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 📄 Licença

Este projeto é fornecido como está para uso pessoal e educacional.

## 🤝 Suporte

Para problemas ou sugestões, verifique a documentação ou entre em contato.

## 📌 Notas

- O aplicativo requer uma API Key válida da OpenAI
- Custos da API são de responsabilidade do usuário
- A qualidade dos resultados depende do modelo escolhido
- Recomenda-se o uso do gpt-4-vision-preview para melhores resultados

## 🔄 Atualizações Futuras

Possíveis melhorias:
- Suporte a mais formatos de imagem
- Comparação lado a lado antes/depois
- Histórico de processamentos
- Batch processing
- Filtros adicionais
- Integração com outros modelos de IA

---

**Versão**: 1.0.0  
**Desenvolvido com**: C#, .NET 8, WPF, MVVM  
**API**: OpenAI GPT-4 Vision
