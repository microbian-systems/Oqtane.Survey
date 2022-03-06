using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.Survey.Migrations.EntityBuilders
{
    public class OqtaneSurveyAnswerEntityBuilder : AuditableBaseEntityBuilder<OqtaneSurveyAnswerEntityBuilder>
    {
        private const string _entityTableName = "OqtaneSurveyAnswer";
        private readonly PrimaryKey<OqtaneSurveyAnswerEntityBuilder> _primaryKey = new("PK_OqtaneSurveyAnswer", x => x.SurveyId);
        //private readonly ForeignKey<OqtaneSurveyEntityBuilder> _moduleForeignKey = new("FK_OqtaneSurvey_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public OqtaneSurveyAnswerEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            //ForeignKeys.Add(_moduleForeignKey);
        }

        protected override OqtaneSurveyAnswerEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SurveyId = AddAutoIncrementColumn(table, "SurveyId");
            UserId = AddIntegerColumn(table, "UserId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            SurveyName = AddMaxStringColumn(table, "SurveyName");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> SurveyId { get; set; }
        public OperationBuilder<AddColumnOperation> UserId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> SurveyName { get; set; }
    }
}
