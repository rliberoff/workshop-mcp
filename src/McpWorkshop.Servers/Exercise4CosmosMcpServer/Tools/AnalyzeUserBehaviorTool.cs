using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Exercise4CosmosMcpServer.Models;

namespace Exercise4CosmosMcpServer.Tools;

public class AnalyzeUserBehaviorTool
{
    public static object GetDefinition()
    {
        return new
        {
            name = "analyze_user_behavior",
            description = "Analizar el comportamiento de un usuario específico",
            inputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["userId"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "ID del usuario a analizar"
                    },
                    ["metricType"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Tipo de métrica (sessions, pageViews, conversions)",
                        ["enum"] = new[] { "sessions", "pageViews", "conversions" },
                        ["default"] = "sessions"
                    }
                },
                ["required"] = new[] { "userId" }
            }
        };
    }

    public static object Execute(Dictionary<string, JsonElement> arguments, UserSession[] sessions, CartEvent[] cartEvents)
    {
        if (!arguments.ContainsKey("userId"))
            throw new ArgumentException("El parámetro 'userId' es requerido");

        var userId = arguments["userId"].GetString() ?? string.Empty;
        var metricType = arguments.ContainsKey("metricType") && arguments["metricType"].ValueKind == JsonValueKind.String
            ? arguments["metricType"].GetString()
            : "sessions";

        var userSessions = sessions.Where(s => s.UserId == userId).ToList();
        var userCartEvents = cartEvents.Where(e => e.UserId == userId).ToList();

        var totalSessions = userSessions.Count;
        var totalPageViews = userSessions.Sum(s => s.PageViews);
        var totalActions = userSessions.Sum(s => s.Actions);
        var hasCheckout = userCartEvents.Any(e => e.Action == "checkout");
        var addToCartCount = userCartEvents.Count(e => e.Action == "addToCart");

        var avgSessionDuration = userSessions.Any()
            ? userSessions.Average(s => (s.SessionEnd - s.SessionStart).TotalMinutes)
            : 0;

        object textContent = new Dictionary<string, object>
        {
            ["type"] = "text",
            ["text"] = $"👤 ANÁLISIS DE COMPORTAMIENTO: {userId}\n\n" +
                       $"Sesiones: {totalSessions}\n" +
                       $"Páginas vistas: {totalPageViews}\n" +
                       $"Acciones: {totalActions}\n" +
                       $"Duración promedio sesión: {avgSessionDuration:F1} minutos\n" +
                       $"Items agregados al carrito: {addToCartCount}\n" +
                       $"Ha realizado checkout: {(hasCheckout ? "Sí" : "No")}\n\n" +
                       $"Métrica solicitada: {metricType}"
        };

        object resourceContent = new Dictionary<string, object>
        {
            ["type"] = "resource",
            ["resource"] = new Dictionary<string, object>
            {
                ["uri"] = $"cosmos://analytics/user-behavior?userId={userId}",
                ["mimeType"] = "application/json",
                ["text"] = JsonSerializer.Serialize(new
                {
                    userId,
                    metrics = new
                    {
                        totalSessions,
                        totalPageViews,
                        totalActions,
                        avgSessionDuration,
                        addToCartCount,
                        hasCheckout
                    },
                    sessions = userSessions,
                    cartEvents = userCartEvents
                })
            }
        };

        return new Dictionary<string, object>
        {
            ["content"] = new[] { textContent, resourceContent }
        };
    }
}
