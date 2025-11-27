using System;
using System.Collections.Generic;
using System.Linq;

namespace MauiApp1.Models.Administrativa
{
    public class AdminModel
    {
        // Identificador (usaremos a coluna "index" conforme combinado)
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;

        public string? DescricaoSituacao { get; set; }

        public string? UnidadeGrupo { get; set; }

        // Propriedades calculadas (mantidas do projeto original)
        public string FotoUrl => $"https://www.gravatar.com/avatar/?d=identicon&s=200";

        // Para exibir email fictício na interface
        public string Email => $"{Nome.ToLower().Replace(" ", ".")}@empresa.com";

        // ==================== PROPRIEDADES CALCULADAS ====================

        public bool EstaAtivo
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return false;

                return DescricaoSituacao.Equals("Trabalhando", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool EstaDemitido
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return false;

                return DescricaoSituacao.Equals("Demitido", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool EstaAposentadoPorInvalidez
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return false;

                return DescricaoSituacao.Equals("Aposentadoria por Invalidez", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool EstaEmAuxilioDoenca
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return false;

                return DescricaoSituacao.Equals("Auxilio Doença", StringComparison.OrdinalIgnoreCase) ||
                       DescricaoSituacao.Equals("Auxílio Doença", StringComparison.OrdinalIgnoreCase);
            }
        }

        public string StatusDescricao
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return "Não Informado";

                return DescricaoSituacao;
            }
        }

        public string StatusEmoji
        {
            get
            {
                if (EstaAtivo) return "✅";
                if (EstaDemitido) return "❌";
                if (EstaAposentadoPorInvalidez) return "🏥";
                if (EstaEmAuxilioDoenca) return "🤕";
                return "❓";
            }
        }

        public string StatusCor
        {
            get
            {
                if (EstaAtivo) return "#4CAF50"; // Verde
                if (EstaDemitido) return "#F44336"; // Vermelho
                if (EstaAposentadoPorInvalidez) return "#9C27B0"; // Roxo
                if (EstaEmAuxilioDoenca) return "#FF9800"; // Laranja
                return "#9E9E9E"; // Cinza
            }
        }

        public string StatusCorFundo
        {
            get
            {
                if (EstaAtivo) return "#E8F5E9";
                if (EstaDemitido) return "#FFEBEE";
                if (EstaAposentadoPorInvalidez) return "#F3E5F5";
                if (EstaEmAuxilioDoenca) return "#FFF3E0";
                return "#F5F5F5";
            }
        }
    }
}
