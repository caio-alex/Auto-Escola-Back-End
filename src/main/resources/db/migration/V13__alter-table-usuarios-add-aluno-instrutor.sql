alter table usuarios add column aluno_id bigint null;
alter table usuarios add column instrutor_id bigint null;

alter table usuarios
    add constraint fk_usuarios_aluno foreign key (aluno_id) references alunos (id);

alter table usuarios
    add constraint fk_usuarios_instrutor foreign key (instrutor_id) references instrutores (id);

-- Um login por aluno/instrutor.
alter table usuarios add constraint uk_usuarios_aluno_id unique (aluno_id);
alter table usuarios add constraint uk_usuarios_instrutor_id unique (instrutor_id);

-- Um usuário nunca pode estar vinculado a um aluno E a um instrutor ao mesmo tempo.
alter table usuarios
    add constraint chk_usuarios_vinculo_unico
        check (aluno_id is null or instrutor_id is null);
