-- Criar banco de testes
CREATE DATABASE IF NOT EXISTS rhsenior_heicomp_test;
USE rhsenior_heicomp_test;

-- Criar tabela 
CREATE TABLE IF NOT EXISTS rhdataset (
                                         id INT AUTO_INCREMENT PRIMARY KEY,
                                         `Descrição (Situação)` VARCHAR(100),
    `Admissão` VARCHAR(20),
    `Data Afastamento` VARCHAR(20)
    );

-- Inserir dados de teste
INSERT INTO rhdataset VALUES
                          (1, 'Ativo', '01/01/2024', NULL),
                          (2, 'Demitido', '01/06/2023', '15/10/2024'),
                          (3, 'Ativo', '15/03/2024', NULL),
                          (4, 'Demitido', '10/01/2023', '20/08/2024');