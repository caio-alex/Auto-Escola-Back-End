package br.com.fiap3esa.autoescola3esa.service;

import br.com.fiap3esa.autoescola3esa.domain.agenda.*;
import br.com.fiap3esa.autoescola3esa.domain.agenda.validacao.ValidadorAgendamento;
import br.com.fiap3esa.autoescola3esa.domain.aluno.Aluno;
import br.com.fiap3esa.autoescola3esa.domain.aluno.AlunoNotFoundException;
import br.com.fiap3esa.autoescola3esa.domain.aluno.AlunoRepository;
import br.com.fiap3esa.autoescola3esa.domain.instrutor.Instrutor;
import br.com.fiap3esa.autoescola3esa.domain.instrutor.InstrutorNotFoundException;
import br.com.fiap3esa.autoescola3esa.domain.instrutor.InstrutorRepository;
import br.com.fiap3esa.autoescola3esa.domain.usuario.Perfil;
import br.com.fiap3esa.autoescola3esa.domain.usuario.Usuario;
import jakarta.persistence.EntityNotFoundException;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.security.access.AccessDeniedException;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Duration;
import java.time.LocalDateTime;
import java.util.List;

@Service
@RequiredArgsConstructor
public class AgendaDeInstrucoes {
    private static final long ANTECEDENCIA_MINIMA_CANCELAMENTO_MINUTOS = 30;

    public final InstrucaoRepository repository;
    public final AlunoRepository alunoRepository;
    public final InstrutorRepository instrutorRepository;
    public final List<ValidadorAgendamento> validadoresAgendamento;

    @Transactional
    public DadosDetalhamentoAgendamento agendar(DadosAgendamentoInstrucao dadosRecebidos, Usuario usuario) {
        final DadosAgendamentoInstrucao dados = aplicarRestricaoDeAluno(dadosRecebidos, usuario);

        if (!alunoRepository.existsById(dados.idAluno())) {
            throw new AlunoNotFoundException("ID do aluno informado não existe!");
        }
        if (dados.idInstrutor() != null && !instrutorRepository.existsById(dados.idInstrutor())) {
            throw new InstrutorNotFoundException("ID do instrutor informado não existe!");
        }
        //Validações
        validadoresAgendamento.forEach(validador -> validador.validar(dados));

        Aluno aluno = alunoRepository.getReferenceById(dados.idAluno());
        Instrutor instrutor = escolherInstrutor(dados);
        if (instrutor == null) {
            throw new ValidacaoException("Nenhum instrutor disponível para a data/hora escolhida!");
        }
        Instrucao instrucao = new Instrucao(
                null,
                aluno,
                instrutor,
                dados.dataHora()
        );
        Instrucao salva = repository.save(instrucao);
        return new DadosDetalhamentoAgendamento(salva);
    }

    public Page<DadosDetalhamentoAgendamento> listar(Pageable paginacao, Usuario usuario) {
        Page<Instrucao> pagina = switch (usuario.getPerfil()) {
            case ALUNO -> repository.findAllByAlunoIdAndCanceladaFalse(usuario.getAluno().getId(), paginacao);
            case INSTRUTOR ->
                    repository.findAllByInstrutorIdAndCanceladaFalse(usuario.getInstrutor().getId(), paginacao);
            case ADMIN, USER -> repository.findAllByCanceladaFalse(paginacao);
        };
        return pagina.map(DadosDetalhamentoAgendamento::new);
    }

    public DadosDetalhamentoAgendamento detalhar(Long id, Usuario usuario) {
        Instrucao instrucao = buscarComPermissao(id, usuario);
        return new DadosDetalhamentoAgendamento(instrucao);
    }

    @Transactional
    public void cancelar(Long id, Usuario usuario) {
        Instrucao instrucao = buscarComPermissao(id, usuario);

        if (instrucao.isCancelada()) {
            throw new ValidacaoException("Esta instrução já está cancelada!");
        }

        long antecedencia = Duration.between(LocalDateTime.now(), instrucao.getDataHora()).toMinutes();
        if (antecedencia < ANTECEDENCIA_MINIMA_CANCELAMENTO_MINUTOS) {
            throw new ValidacaoException("Cancelamento deve ocorrer com no mínimo 30 min de antecedência!");
        }

        instrucao.cancelar();
        repository.save(instrucao);
    }

    /**
     * Um ALUNO só pode agendar para si mesmo. Se o id_aluno enviado não bater
     * com o próprio aluno vinculado ao usuário logado, rejeitamos - ao invés
     * de simplesmente sobrescrever em silêncio, o que esconderia um possível
     * erro (ou tentativa indevida) do lado de quem chama a API.
     */
    private DadosAgendamentoInstrucao aplicarRestricaoDeAluno(DadosAgendamentoInstrucao dados, Usuario usuario) {
        if (usuario.getPerfil() != Perfil.ALUNO) {
            return dados;
        }
        Long idProprioAluno = usuario.getAluno().getId();
        if (dados.idAluno() != null && !dados.idAluno().equals(idProprioAluno)) {
            throw new AccessDeniedException("Você só pode agendar instruções para si mesmo.");
        }
        return new DadosAgendamentoInstrucao(
                idProprioAluno,
                dados.idInstrutor(),
                dados.especialidade(),
                dados.dataHora()
        );
    }

    private Instrucao buscarComPermissao(Long id, Usuario usuario) {
        Instrucao instrucao = repository.findById(id)
                .orElseThrow(EntityNotFoundException::new);

        boolean donoDivergente = switch (usuario.getPerfil()) {
            case ALUNO -> !instrucao.getAluno().getId().equals(usuario.getAluno().getId());
            case INSTRUTOR -> !instrucao.getInstrutor().getId().equals(usuario.getInstrutor().getId());
            case ADMIN, USER -> false;
        };
        if (donoDivergente) {
            throw new AccessDeniedException("Esta instrução não pertence a você.");
        }

        return instrucao;
    }

    private Instrutor escolherInstrutor(DadosAgendamentoInstrucao dados) {
        if (dados.idInstrutor() != null) {
            return instrutorRepository.getReferenceById(dados.idInstrutor());
        }
        if (dados.especialidade() == null) {
            throw new ValidacaoException("Especialidade é campo obrigatório, caso o instrutor não seja informado!");
        }
        return instrutorRepository.escolherInstrutorAleatorioDisponivel(
                dados.especialidade(),
                dados.dataHora()
        );
    }
}
