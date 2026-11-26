using Microsoft.EntityFrameworkCore;
using EventosApi.Models;
using EventosApi.Models.Enuns;

namespace EventosApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Organizador> TB_ORGANIZADORES { get; set; }
        public DbSet<Local> TB_LOCAIS { get; set; }
        public DbSet<Evento> TB_EVENTOS { get; set; }
        public DbSet<Participante> TB_PARTICIPANTES { get; set; }
        public DbSet<Inscricao> TB_INSCRICOES { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Organizador 1:N Eventos (não permite apagar organizador com eventos)
            modelBuilder.Entity<Evento>()
                .HasOne(e => e.Organizador)
                .WithMany(o => o.Eventos)
                .HasForeignKey(e => e.OrganizadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Local 1:N Eventos (não permite apagar local com eventos)
            modelBuilder.Entity<Evento>()
                .HasOne(e => e.Local)
                .WithMany(l => l.Eventos)
                .HasForeignKey(e => e.LocalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Evento 1:N Inscrições (apagar o evento apaga as inscrições)
            modelBuilder.Entity<Inscricao>()
                .HasOne(i => i.Evento)
                .WithMany(e => e.Inscricoes)
                .HasForeignKey(i => i.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Participante 1:N Inscrições (apagar o participante apaga as inscrições)
            modelBuilder.Entity<Inscricao>()
                .HasOne(i => i.Participante)
                .WithMany(p => p.Inscricoes)
                .HasForeignKey(i => i.ParticipanteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Um participante só se inscreve uma vez por evento
            modelBuilder.Entity<Inscricao>()
                .HasIndex(i => new { i.EventoId, i.ParticipanteId })
                .IsUnique();

            // ---------------- SEED ----------------
            var cadastro = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Organizador>().HasData(
                new Organizador { Id = 1, Nome = "Mariana Souza", Email = "mariana@eventos.com", Telefone = "(11) 91234-5678", Empresa = "Mariana Eventos", DataCadastro = cadastro },
                new Organizador { Id = 2, Nome = "Carlos Lima",   Email = "carlos@eventos.com",  Telefone = "(11) 98765-4321", Empresa = "CL Produções",    DataCadastro = cadastro }
            );

            modelBuilder.Entity<Local>().HasData(
                new Local { Id = 1, Nome = "Auditório Central", Endereco = "Rua Augusta, 1500",      Cidade = "São Paulo", Capacidade = 200 },
                new Local { Id = 2, Nome = "Parque Municipal",  Endereco = "Av. Paulista, 900",       Cidade = "São Paulo", Capacidade = 500 }
            );

            modelBuilder.Entity<Evento>().HasData(
                new Evento
                {
                    Id = 1, Titulo = "Workshop de Tecnologia", Descricao = "Palestras e oficinas de programação.",
                    DataInicio = new DateTime(2027, 3, 15, 19, 0, 0, DateTimeKind.Utc),
                    DataFim = new DateTime(2027, 3, 15, 22, 0, 0, DateTimeKind.Utc),
                    Preco = 50m, CapacidadeMaxima = 150, Status = StatusEventoEnum.Publicado,
                    DataCriacao = cadastro, OrganizadorId = 1, LocalId = 1
                },
                new Evento
                {
                    Id = 2, Titulo = "Feira de Empreendedorismo", Descricao = "Estandes, networking e mentorias.",
                    DataInicio = new DateTime(2027, 4, 20, 9, 0, 0, DateTimeKind.Utc),
                    DataFim = new DateTime(2027, 4, 20, 18, 0, 0, DateTimeKind.Utc),
                    Preco = 0m, CapacidadeMaxima = 300, Status = StatusEventoEnum.Publicado,
                    DataCriacao = cadastro, OrganizadorId = 1, LocalId = 2
                },
                new Evento
                {
                    Id = 3, Titulo = "Festival de Música", Descricao = "Shows ao ar livre com bandas locais.",
                    DataInicio = new DateTime(2027, 5, 30, 16, 0, 0, DateTimeKind.Utc),
                    DataFim = new DateTime(2027, 5, 30, 23, 0, 0, DateTimeKind.Utc),
                    Preco = 80m, CapacidadeMaxima = 400, Status = StatusEventoEnum.Rascunho,
                    DataCriacao = cadastro, OrganizadorId = 2, LocalId = 2
                }
            );

            modelBuilder.Entity<Participante>().HasData(
                new Participante { Id = 1, Nome = "Ana Paula",  Email = "ana@email.com",   Telefone = "(11) 90000-0001" },
                new Participante { Id = 2, Nome = "Bruno Reis", Email = "bruno@email.com", Telefone = "(11) 90000-0002" },
                new Participante { Id = 3, Nome = "Carla Dias", Email = "carla@email.com", Telefone = "(11) 90000-0003" },
                new Participante { Id = 4, Nome = "Diego Melo", Email = "diego@email.com", Telefone = "(11) 90000-0004" }
            );

            modelBuilder.Entity<Inscricao>().HasData(
                new Inscricao { Id = 1, EventoId = 1, ParticipanteId = 1, DataInscricao = cadastro, Status = StatusInscricaoEnum.Confirmada },
                new Inscricao { Id = 2, EventoId = 1, ParticipanteId = 2, DataInscricao = cadastro, Status = StatusInscricaoEnum.Confirmada },
                new Inscricao { Id = 3, EventoId = 2, ParticipanteId = 3, DataInscricao = cadastro, Status = StatusInscricaoEnum.Confirmada },
                new Inscricao { Id = 4, EventoId = 2, ParticipanteId = 1, DataInscricao = cadastro, Status = StatusInscricaoEnum.Confirmada }
            );
        }

        
    }
}
