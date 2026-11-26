# EventosApi - Gerenciamento de Eventos (.NET 8)

API RESTful (CRUD) em que o **organizador** cria e gerencia eventos, locais e inscrições.
Armazenamento: Entity Framework Core InMemory (dados recriados a cada execução).

## Executar
    dotnet restore
    dotnet run

API: http://localhost:5180 | Swagger: http://localhost:5180/swagger
Postman: importe `EventosApi.postman_collection.json` (variável `baseUrl`).

## Rotas
| Recurso | CRUD | Extras |
|---|---|---|
| /Organizadores | GET, GET/{id}, POST, PUT/{id}, DELETE/{id} | GET /{id}/Eventos, GET /{id}/Resumo |
| /Locais | idem | filtro ?cidade= |
| /Eventos | idem | filtros ?status=&organizadorId=&de=&ate=&busca=; GET /{id}/Inscricoes, GET /{id}/Resumo, POST /{id}/Publicar, POST /{id}/Cancelar |
| /Participantes | idem | filtro ?busca= |
| /Inscricoes | idem | filtros ?eventoId=&participanteId=&status= |

## Enviar para o GitHub
    git init
    git branch -M main
    git remote add origin https://github.com/GhostKa23/ProjetoCrudAPI.git
    git add .
    git commit -m "API de gerenciamento de eventos (CRUD)"
    git push -u origin main
