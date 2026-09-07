# MindSync — Web

Frontend do MindSync em React + TypeScript + Vite: uma plataforma de mixagem sonora (frequências binaurais + texturas ambiente) para mitigação de estresse ocupacional, com autenticação, criação/histórico de sessões, player com mixer de áudio em tempo real, listas de favoritos e um painel de insights sobre a evolução do estresse.

## Stack

- **React 19** + **React Router 7**
- **TypeScript**
- **Vite 8**
- **Tailwind CSS 4**
- **Framer Motion** (animações do cartão de autenticação)
- **Web Audio API** nativa (sem biblioteca externa) para o motor de mixagem do player

## Como rodar

### Pré-requisitos
- Node.js 20+
- A API rodando (ver o README do backend na pasta `MindSync/`)

### Passo a passo

```bash
npm install
cp .env.example .env
npm run dev
```

O `.env` só precisa apontar para onde a API está rodando (por padrão já vem certo se você seguiu o setup padrão do backend):

```env
VITE_API_URL=http://localhost:5000/api
```

## Arquitetura

Código organizado por responsabilidade (não por página), para evitar duplicação e manter arquivos pequenos e fáceis de testar:

```
src/
├── components/
│   ├── auth/          — reutilizados por Login e Register
|   ├── common/
│   ├── dialog/        — substitui window.confirm/alert nativos
│   ├── favorites/      
│   ├── home/            
│   ├── icons/         — ícones SVG reutilizáveis
│   ├── insights/        
│   ├── modal/         — base dos modais de sessão
│   ├── player/          
│   └── session/
├── constants/          
├── contexts/            
├── hooks/               
├── pages/             — só orquestram, sem lógica de API nem estilos inline
├── services/            
├── types/             — tipos compartilhados
└── utils/             — funções puras
```

### Camada de serviços

Toda chamada de rede passa por `services/api.ts` (`apiFetch`), que resolve `credentials: 'include'` (necessário para o cookie `HttpOnly` de autenticação), cabeçalhos, e desembrulha o envelope `ApiResponse<T>` que o backend sempre retorna, tratando erros de validação e de rede (offline vs. servidor fora do ar) de forma consistente em toda a aplicação.

### Player e motor de mixagem (`useAudioMixEngine`)

O player consome a mixagem retornada por `GET /api/session-audio-mixes/{stressSessionId}` (uma faixa binaural fixa pela frequência-alvo da sessão + uma textura/ambiente compatível com o nível de estresse relatado) e usa a Web Audio API diretamente:

- Baixa e decodifica todas as camadas de áudio em paralelo (com progresso real via stream).
- Inicia a reprodução em loop *sample-accurate* (sem o "clique" que `<audio loop>` costuma ter em mp3).
- Expõe um `AnalyserNode` por camada, lido diretamente num loop de `requestAnimationFrame` fora do ciclo de render do React, que alimenta a animação reativa ao som em `ReactiveBackground`.
- Permite ajustar o volume de cada camada independentemente em tempo real (`MixerPanel`).

### Favoritos: listas de sessões

O usuário pode organizar sessões de estresse em listas nomeadas de favoritos:

- **Estrela no card**: abre um popover para marcar/desmarcar em quais listas aquela sessão deve entrar, ou criar uma lista nova já favoritando nela.
- **Drawer lateral**: lista todas as listas do usuário: criar, renomear, excluir listas, e dentro de cada uma, renomear ou remover cada sessão favoritada.

### Insights

A página `Insights` consome o histórico de sessões avaliadas e calcula, no cliente (`utils/insights.ts`), a evolução do estresse ao longo do tempo, a redução média e qual frequência-alvo trouxe melhor resultado para aquele usuário (sem endpoint dedicado no backend, tudo derivado do mesmo histórico já usado na Home).

## Decisões de design

- **Sem `window.alert`/`window.confirm` nativos**: substituídos por `DialogProvider` + `ConfirmDialog`, mantendo o visual consistente com o resto da aplicação.
- **Nenhum estado de autenticação em `localStorage`**: o token nunca circula pelo JavaScript da página, só via cookie `HttpOnly` setado pelo backend, o que evita roubo de sessão via XSS.
- **Componentização por responsabilidade, não por página**: evita duplicação de lógica de mixer, modal, favoritos e formulário entre as diferentes telas que os usam.