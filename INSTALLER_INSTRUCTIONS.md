# Instruções para Criar Instalador Windows

## Método 1: Usando WiX Toolset (Recomendado)

### Pré-requisitos

1. Instale o [WiX Toolset](https://wixtoolset.org/releases/)
2. Instale o [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community Edition é gratuito)
3. Instale o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Passos

1. **Abrir o Projeto no Visual Studio**
   - Abra a solução `AIImageChat.sln`
   - Adicione o projeto `AIImageChat.Setup.wixproj` à solução

2. **Configurar Dependências**
   - No arquivo `AIImageChat.Setup.wixproj`, substitua `YOUR-GUID-HERE` por um GUID real
   - Gere um GUID usando: `Tools -> Create GUID` no Visual Studio
   - Substitua `YOUR-PROJECT-GUID-HERE` pelo GUID do projeto AIImageChat

3. **Compilar o Projeto Principal**
   - Compile o projeto `AIImageChat` em modo Release
   - Certifique-se de que todos os arquivos estão em `bin\Release\net8.0-windows`

4. **Compilar o Instalador**
   - Compile o projeto `AIImageChat.Setup` em modo Release
   - O arquivo MSI será gerado em `AIImageChat.Setup\bin\Release\`

5. **Testar o Instalador**
   - Execute o arquivo MSI gerado
   - Verifique se a instalação funciona corretamente
   - Teste o desinstalador no Painel de Controle

## Método 2: Usando Inno Setup (Alternativa Simples)

### Pré-requisitos

1. Baixe e instale o [Inno Setup](https://jrsoftware.org/isdl.php)

### Passos

1. **Criar Script de Instalação**
   - Crie um arquivo `AIImageChat.iss` com o conteúdo abaixo

2. **Compilar Script**
   - Abra o arquivo `.iss` no Inno Setup Compiler
   - Pressione F9 para compilar
   - O instalador EXE será gerado

### Script Inno Setup (AIImageChat.iss)

```iss
[Setup]
AppName=AI Image Chat
AppVersion=1.0.0
DefaultDirName={pf}\AI Image Chat
DefaultGroupName=AI Image Chat
OutputBaseFilename=AIImageChat_Setup
Compression=zip
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
UninstallDisplayIcon={app}\AIImageChat.exe

[Files]
; Arquivos principais
Source: "AIImageChat\bin\Release\net8.0-windows\AIImageChat.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "AIImageChat\bin\Release\net8.0-windows\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "AIImageChat\bin\Release\net8.0-windows\*.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "AIImageChat\bin\Release\net8.0-windows\*.json"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\AI Image Chat"; Filename: "{app}\AIImageChat.exe"
Name: "{group}\Uninstall AI Image Chat"; Filename: "{uninstallexe}"
Name: "{commondesktop}\AI Image Chat"; Filename: "{app}\AIImageChat.exe"

[Run]
Filename: "{app}\AIImageChat.exe"; Description: "Launch AI Image Chat"; Flags: nowait postinstall skipifsilent
```

## Método 3: Usando dotnet-publish (Self-Contained)

### Passos

1. **Publicar Aplicação Self-Contained**
   ```bash
   cd AIImageChat
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
   ```

2. **Empacotar com ZIP**
   - Vá para `bin\Release\net8.0-windows\win-x64\publish`
   - Compacte todos os arquivos em um ZIP
   - Renomeie para `AIImageChat_Portable.zip`

3. **Criar Instalador Simples com NSIS**
   - Baixe o [NSIS](https://nsis.sourceforge.io/)
   - Crie um script simples para extrair o ZIP
   - Compile para gerar o EXE

## Método 4: Usando Advanced Installer (GUI)

### Pré-requisitos

1. Baixe o [Advanced Installer](https://www.advancedinstaller.com/)

### Passos

1. **Criar Novo Projeto**
   - Abra o Advanced Installer
   - Selecione "Simple" ou "Professional" project template
   - Nome do produto: "AI Image Chat"
   - Versão: 1.0.0

2. **Adicionar Arquivos**
   - Vá para "Files and Folders"
   - Adicione todos os arquivos de `bin\Release\net8.0-windows`
   - Configure o diretório de instalação

3. **Configurar Atalhos**
   - Vá para "Shortcuts"
   - Adicione atalho no Desktop
   - Adicione atalho no Menu Iniciar

4. **Configurar Requisitos**
   - Vá para "Prerequisites"
   - Adicione ".NET 8.0 Runtime" como prerequisite

5. **Compilar**
   - Clique em "Build"
   - O instalador MSI/EXE será gerado

## Verificação do .NET 8 Runtime

Para verificar se o .NET 8 Runtime está instalado no sistema do usuário, adicione uma launch condition no WiX:

```xml
<Property Id="NET8RUNTIME">
  <RegistrySearch Id="Net8Search" 
                  Root="HKLM" 
                  Key="SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" 
                  Name="Release" 
                  Type="raw" 
                  Win64="no">
    <DirectorySearch Id="Net8Path" Path="Microsoft.NET\Framework64\v4.0.30319" />
  </RegistrySearch>
</Property>
```

## Distribuinte

### Opcional: Criar instalador com bootstrapper

Para incluir o .NET 8 Runtime no instalador, use o bootstrapper:

1. Crie um projeto "Setup Project" no Visual Studio
2. Adicione o .NET 8 Runtime como prerequisite
3. Configure para baixar automaticamente se não estiver instalado

## Notas Importantes

- **GUIDs**: Substitua todos os placeholders `YOUR-GUID-HERE` por GUIDs reais
- **Caminhos**: Ajuste os caminhos conforme sua estrutura de diretórios
- **Assinatura**: Para distribuição pública, considere assinar o instalador com um certificado digital
- **Teste**: Sempre teste o instalador em uma máquina limpa antes de distribuir

## Solução de Problemas

### Erro: "Cannot find WiX targets"
- Verifique se o WiX Toolset está instalado corretamente
- Verifique o caminho em `WixTargetsPath`

### Erro: "Missing reference"
- Certifique-se de que o projeto AIImageChat compila corretamente
- Verifique se todos os arquivos estão em `bin\Release\net8.0-windows`

### Erro: "Access denied"
- Execute o Visual Studio como Administrador
- Verifique as permissões da pasta de saída

## Recomendação

Para a maioria dos casos, o **Método 2 (Inno Setup)** é o mais simples e eficaz:
- Fácil de usar
- Gera instalador EXE compacto
- Interface moderna
- Suporte para prerequisites
- Bem documentado
