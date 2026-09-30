package br.com.fiap3esa.autoescola3esa.domain.usuario;

public record DadosDetalhamentoUsuario(
        Long id,
        String login,
        Perfil perfil,
        Long idAluno,
        Long idInstrutor) {
    public DadosDetalhamentoUsuario(Usuario usuario) {
        this(
                usuario.getId(),
                usuario.getLogin(),
                usuario.getPerfil(),
                usuario.getAluno() != null ? usuario.getAluno().getId() : null,
                usuario.getInstrutor() != null ? usuario.getInstrutor().getId() : null
        );
    }
}
