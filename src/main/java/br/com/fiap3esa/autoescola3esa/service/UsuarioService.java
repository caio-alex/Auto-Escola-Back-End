package br.com.fiap3esa.autoescola3esa.service;

import br.com.fiap3esa.autoescola3esa.domain.agenda.ValidacaoException;
import br.com.fiap3esa.autoescola3esa.domain.aluno.Aluno;
import br.com.fiap3esa.autoescola3esa.domain.aluno.AlunoNotFoundException;
import br.com.fiap3esa.autoescola3esa.domain.aluno.AlunoRepository;
import br.com.fiap3esa.autoescola3esa.domain.instrutor.Instrutor;
import br.com.fiap3esa.autoescola3esa.domain.instrutor.InstrutorNotFoundException;
import br.com.fiap3esa.autoescola3esa.domain.instrutor.InstrutorRepository;
import br.com.fiap3esa.autoescola3esa.domain.usuario.*;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
public class UsuarioService {
    private final UsuarioRepository repository;
    private final AlunoRepository alunoRepository;
    private final InstrutorRepository instrutorRepository;
    private final PasswordEncoder passwordEncoder;

    @Transactional
    public DadosDetalhamentoUsuario cadastrarUsuario(DadosCadastroUsuario dados) {
        if (repository.existsByLogin(dados.login())) {
            throw new ValidacaoException("Já existe um usuário cadastrado com esse login!");
        }

        Perfil perfil = dados.perfil() != null ? dados.perfil() : Perfil.USER;
        Aluno aluno = resolverAluno(perfil, dados.idAluno());
        Instrutor instrutor = resolverInstrutor(perfil, dados.idInstrutor());

        String senhaCodificada = passwordEncoder.encode(dados.senha());
        Usuario usuario = new Usuario(dados.login(), senhaCodificada, perfil, aluno, instrutor);
        Usuario saved = repository.save(usuario);
        return new DadosDetalhamentoUsuario(saved);
    }

    public Page<DadosDetalhamentoUsuario> listarUsuarios(Pageable paginacao) {
        return repository
                .findAll(paginacao)
                .map(DadosDetalhamentoUsuario::new);
    }

    public DadosDetalhamentoUsuario detalharUsuarioLogado(Usuario usuario) {
        return new DadosDetalhamentoUsuario(usuario);
    }

    private Aluno resolverAluno(Perfil perfil, Long idAluno) {
        if (perfil != Perfil.ALUNO) {
            if (idAluno != null) {
                throw new ValidacaoException("id_aluno só deve ser informado para o perfil ALUNO!");
            }
            return null;
        }
        if (idAluno == null) {
            throw new ValidacaoException("id_aluno é obrigatório para o perfil ALUNO!");
        }
        if (!alunoRepository.existsById(idAluno)) {
            throw new AlunoNotFoundException("ID do aluno informado não existe!");
        }
        if (repository.existsByAlunoId(idAluno)) {
            throw new ValidacaoException("Este aluno já possui um usuário vinculado!");
        }
        return alunoRepository.getReferenceById(idAluno);
    }

    private Instrutor resolverInstrutor(Perfil perfil, Long idInstrutor) {
        if (perfil != Perfil.INSTRUTOR) {
            if (idInstrutor != null) {
                throw new ValidacaoException("id_instrutor só deve ser informado para o perfil INSTRUTOR!");
            }
            return null;
        }
        if (idInstrutor == null) {
            throw new ValidacaoException("id_instrutor é obrigatório para o perfil INSTRUTOR!");
        }
        if (!instrutorRepository.existsById(idInstrutor)) {
            throw new InstrutorNotFoundException("ID do instrutor informado não existe!");
        }
        if (repository.existsByInstrutorId(idInstrutor)) {
            throw new ValidacaoException("Este instrutor já possui um usuário vinculado!");
        }
        return instrutorRepository.getReferenceById(idInstrutor);
    }
}
