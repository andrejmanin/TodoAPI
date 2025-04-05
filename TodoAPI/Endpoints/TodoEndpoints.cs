using Dapper;
using Microsoft.Data.SqlClient;
using TodoAPI.Models;
using TodoAPI.Services;

namespace TodoAPI.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/todo-list", async (SqlConnectionFactory sqlConnectionFactory) =>
        {
            using var connection = sqlConnectionFactory.Create();
            const string sql = "SELECT * FROM dbo.[Todo];";

            var todoes = await connection.QueryAsync<Todo>(sql);
            return Results.Ok(todoes);
        });

        builder.MapGet("/todo-list/{id}", async (int id, SqlConnectionFactory sqlConnectionFactory) =>
        {
            using var connection = sqlConnectionFactory.Create();
            const string sql = @"
                SELECT * FROM dbo.[Todo]
                WHERE Id = @TodoId;
            ";

            var todoes = await connection.QuerySingleOrDefaultAsync<Todo>(sql, new { TodoId = id });
            return todoes is not null ? Results.Ok(todoes) : Results.NotFound();
        });

        builder.MapPost("/todo-list", async (Todo todo, SqlConnectionFactory sqlConnectionFactory) =>
        {
            using var connection = sqlConnectionFactory.Create();
            const string sql = @"
                INSERT INTO Todo (Title, Description, IsComplete) 
                VALUES (@Title, @Description, @IsComplete);
            ";
            await connection.ExecuteAsync(sql, todo);
            return Results.Ok();
        });

        builder.MapPut("/todo-list", async (Todo todo, SqlConnectionFactory sqlConnectionFactory) =>
        {
            using var connection = sqlConnectionFactory.Create();
            string sql;
            if (todo.IsComplete == 0)
            {
                 sql = @"
                    UPDATE Todo 
                    SET Title = @Title, Description = @Description, IsComplete = @IsComplete
                    WHERE Id = @Id;
                ";
                await connection.ExecuteAsync(sql, todo);   
            }
            else
            {
                sql = "DELETE FROM Todo WHERE Id = @Id;";
                await connection.ExecuteAsync(sql, new { Id = todo.Id });
            }
            return Results.Ok();
        });

        builder.MapDelete("/todo-list/{id}", async (int id, SqlConnectionFactory sqlConnectionFactory) =>
        {
            using var connection = sqlConnectionFactory.Create();
            const string sql = "DELETE FROM Todo WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, new { Id = id });
            return Results.Ok();
        });
    }
}