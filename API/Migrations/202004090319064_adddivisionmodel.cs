namespace API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adddivisionmodel : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Department", newName: "TB_M_Department");
            CreateTable(
                "dbo.TB_M_Division",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DivisionName = c.String(),
                        IsDelete = c.Boolean(nullable: false),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdateDate = c.DateTimeOffset(precision: 7),
                        DeleteDate = c.DateTimeOffset(precision: 7),
                        Department_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TB_M_Department", t => t.Department_Id, cascadeDelete: true)
                .Index(t => t.Department_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TB_M_Division", "Department_Id", "dbo.TB_M_Department");
            DropIndex("dbo.TB_M_Division", new[] { "Department_Id" });
            DropTable("dbo.TB_M_Division");
            RenameTable(name: "dbo.TB_M_Department", newName: "Department");
        }
    }
}
