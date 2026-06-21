BEGIN TRANSACTION;
EXEC sp_rename N'[Receita]', N'Prescricao', 'OBJECT';

EXEC sp_rename N'[ReceitaMed]', N'PrescricaoMed', 'OBJECT';

EXEC sp_rename N'[ReceitaCID]', N'PrescricaoCID', 'OBJECT';

EXEC sp_rename N'PK_Receita', N'PK_Prescricao';

EXEC sp_rename N'[PrescricaoMed].[ReceitaID]', N'PrescricaoID', 'COLUMN';

EXEC sp_rename N'[PrescricaoCID].[ReceitaID]', N'PrescricaoID', 'COLUMN';

EXEC sp_rename N'[EvolucaoClinica].[ReceitaID]', N'PrescricaoID', 'COLUMN';

EXEC sp_rename N'[InstrucoesMed].[Med_ReceitaID]', N'Med_PrescricaoID', 'COLUMN';

EXEC sp_rename N'[Prescricao].[IX_Receita_PacienteID]', N'IX_Prescricao_PacienteID', 'INDEX';

EXEC sp_rename N'[Prescricao].[IX_Receita_UsuarioCriacaoID]', N'IX_Prescricao_UsuarioCriacaoID', 'INDEX';

EXEC sp_rename N'[PrescricaoMed].[IX_ReceitaMed_CategoriaID]', N'IX_PrescricaoMed_CategoriaID', 'INDEX';

EXEC sp_rename N'[PrescricaoMed].[IX_ReceitaMed_FrequenciaID]', N'IX_PrescricaoMed_FrequenciaID', 'INDEX';

EXEC sp_rename N'[PrescricaoMed].[IX_ReceitaMed_MedicamentoID]', N'IX_PrescricaoMed_MedicamentoID', 'INDEX';

EXEC sp_rename N'[PrescricaoMed].[IX_ReceitaMed_ReceitaID]', N'IX_PrescricaoMed_PrescricaoID', 'INDEX';

EXEC sp_rename N'[PrescricaoMed].[IX_ReceitaMed_TipoID]', N'IX_PrescricaoMed_TipoID', 'INDEX';

EXEC sp_rename N'[PrescricaoCID].[IX_ReceitaCID_CategoriaCID_ID]', N'IX_PrescricaoCID_CategoriaCID_ID', 'INDEX';

EXEC sp_rename N'[PrescricaoCID].[IX_ReceitaCID_ReceitaID]', N'IX_PrescricaoCID_PrescricaoID', 'INDEX';

EXEC sp_rename N'[EvolucaoClinica].[IX_EvolucaoClinica_ReceitaID]', N'IX_EvolucaoClinica_PrescricaoID', 'INDEX';

EXEC sp_rename N'[InstrucoesMed].[IX_InstrucoesMed_Med_ReceitaID]', N'IX_InstrucoesMed_Med_PrescricaoID', 'INDEX';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260616001705_RenomearReceitaParaPrescricao', N'9.0.0');

COMMIT;
GO

