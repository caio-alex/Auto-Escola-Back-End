package br.com.fiap3esa.autoescola3esa.domain.aluno;

import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;

public interface AlunoRepository extends JpaRepository<Aluno, Long> {
    boolean existsByIdAndAtivoTrue(Long id);

    Page<Aluno> findAllByAtivoTrue(Pageable paginacao);
}
