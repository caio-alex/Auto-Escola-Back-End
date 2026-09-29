package br.com.fiap3esa.autoescola3esa.domain.agenda;

import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;

import java.time.LocalDateTime;

public interface InstrucaoRepository extends JpaRepository<Instrucao, Long> {
    boolean existsByInstrutorIdAndDataHoraAndCanceladaFalse(Long idInstrutor, LocalDateTime dataHora);

    boolean existsByAlunoIdAndDataHoraBetweenAndCanceladaFalse(Long idAluno, LocalDateTime inicio, LocalDateTime fim);

    Page<Instrucao> findAllByCanceladaFalse(Pageable paginacao);
}
