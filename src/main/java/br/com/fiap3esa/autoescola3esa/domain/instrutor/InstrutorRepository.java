package br.com.fiap3esa.autoescola3esa.domain.instrutor;

import jakarta.validation.constraints.Future;
import jakarta.validation.constraints.NotNull;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;

import java.time.LocalDateTime;

public interface InstrutorRepository extends JpaRepository<Instrutor, Long> {
    Page<Instrutor> findAllByAtivoTrue(Pageable paginacao);

    @Query("""
        select i from Instrutor i
        where
        i.ativo = true
        and
        i.especialidade = :especialidade
        and
        i.id not in(
                select a.instrutor.id from Instrucao a
                where
                a.dataHora = :dataHora
                and
                a.cancelada = false
        )
        order by rand()
        limit 1    
    """)
    Instrutor escolherInstrutorAleatorioDisponivel(Especialidade especialidade, LocalDateTime dataHora);

    boolean existsByIdAndAtivoTrue(Long id);
}