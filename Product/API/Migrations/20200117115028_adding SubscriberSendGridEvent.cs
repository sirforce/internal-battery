﻿using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSubscriberSendGridEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriberSendGridEvent",
                columns: table => new
                {
                    SubscriberSendGridEventId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberSendGridEventGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    Event = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    EventStatus = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Marketing_campaign_id = table.Column<string>(nullable: true),
                    Marketing_campaign_name = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Sg_message_id = table.Column<string>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Attempt = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberSendGridEvent", x => x.SubscriberSendGridEventId);
                    table.ForeignKey(
                        name: "FK_SubscriberSendGridEvent_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberSendGridEvent_SubscriberId",
                table: "SubscriberSendGridEvent",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberSendGridEvent");
        }
    }
}
