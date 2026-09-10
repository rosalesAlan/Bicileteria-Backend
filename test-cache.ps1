#!/usr/bin/env pwsh
# Script de prueba del MongoCacheService
# Ejecución: powershell -ExecutionPolicy Bypass .\test-cache.ps1

Write-Host "=== Prueba de MongoCacheService ===" -ForegroundColor Cyan

# Variables
$BASE_URL = "https://localhost:5001"
$TOKEN = "REEMPLAZA_CON_JWT_TOKEN"

# Función para hacer requests
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null
    )

    $headers = @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $TOKEN"
    }

    $uri = "$BASE_URL$Endpoint"

    try {
        if ($Body) {
            Invoke-WebRequest -Method $Method -Uri $uri -Headers $headers -Body ($Body | ConvertTo-Json) -SkipCertificateCheck
        } else {
            Invoke-WebRequest -Method $Method -Uri $uri -Headers $headers -SkipCertificateCheck
        }
    }
    catch {
        Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Test 1: Verificar estado del caché (inicial)
Write-Host "`n[TEST 1] Estado del caché - ANTES de primera consulta" -ForegroundColor Yellow
$response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/products/cache/status"
if ($response) {
    $content = $response.Content | ConvertFrom-Json
    Write-Host "? Respuesta:" -ForegroundColor Green
    Write-Host ($content | ConvertTo-Json -Depth 5 | Out-String)
}

# Test 2: Obtener productos (fuerza carga desde BD)
Write-Host "`n[TEST 2] Obtener productos - Primera consulta (MISS)" -ForegroundColor Yellow
$response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/products"
if ($response) {
    $products = $response.Content | ConvertFrom-Json
    Write-Host "? Productos obtenidos: $($products.Count)" -ForegroundColor Green
}

# Test 3: Verificar estado del caché (después de primera consulta)
Write-Host "`n[TEST 3] Estado del caché - DESPUÉS de primera consulta" -ForegroundColor Yellow
$response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/products/cache/status"
if ($response) {
    $content = $response.Content | ConvertFrom-Json
    Write-Host "? Estado del caché:" -ForegroundColor Green
    Write-Host "  - Estatus: $($content.status)" -ForegroundColor Cyan
    Write-Host "  - Productos en caché: $($content.productosCacheados)" -ForegroundColor Cyan
    Write-Host "  - Expirado: $($content.estaExpirado)" -ForegroundColor Cyan
    Write-Host "  - Próxima expiración: $($content.proximaExpiracion)" -ForegroundColor Cyan
}

# Test 4: Obtener productos (desde caché - HIT)
Write-Host "`n[TEST 4] Obtener productos - Segunda consulta (HIT desde caché)" -ForegroundColor Yellow
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/products"
$stopwatch.Stop()
if ($response) {
    $products = $response.Content | ConvertFrom-Json
    Write-Host "? Productos obtenidos: $($products.Count)" -ForegroundColor Green
    Write-Host "  - Tiempo de respuesta: $($stopwatch.ElapsedMilliseconds)ms (desde caché - debe ser rápido)" -ForegroundColor Cyan
}

# Test 5: Invalidar caché
Write-Host "`n[TEST 5] Invalidar caché manualmente" -ForegroundColor Yellow
$response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/products/cache/invalidate"
if ($response) {
    $content = $response.Content | ConvertFrom-Json
    Write-Host "? $($content.message)" -ForegroundColor Green
}

# Test 6: Verificar que caché fue eliminada
Write-Host "`n[TEST 6] Estado del caché - DESPUÉS de invalidación" -ForegroundColor Yellow
$response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/products/cache/status"
if ($response) {
    $content = $response.Content | ConvertFrom-Json
    Write-Host "? Estatus: $($content.status)" -ForegroundColor Green
    if ($content.status -eq "sin_cache") {
        Write-Host "  ? Caché invalidada correctamente" -ForegroundColor Green
    }
}

# Test 7: Crear producto (reconstruye caché)
Write-Host "`n[TEST 7] Crear nuevo producto" -ForegroundColor Yellow
$newProduct = @{
    name = "Bicicleta Prueba"
    description = "Bicicleta de prueba del servicio de caché"
    price = 999.99
    imageUrl = "https://example.com/test.jpg"
    categoryId = 1
    availability = $true
}

$response = Invoke-ApiRequest -Method "POST" -Endpoint "/api/products" -Body $newProduct
if ($response) {
    $product = $response.Content | ConvertFrom-Json
    Write-Host "? Producto creado con ID: $($product.id)" -ForegroundColor Green
}

# Test 8: Verificar que caché fue reconstruida
Write-Host "`n[TEST 8] Estado del caché - DESPUÉS de crear producto" -ForegroundColor Yellow
$response = Invoke-ApiRequest -Method "GET" -Endpoint "/api/products/cache/status"
if ($response) {
    $content = $response.Content | ConvertFrom-Json
    Write-Host "? Estatus: $($content.status)" -ForegroundColor Green
    Write-Host "  - Productos en caché: $($content.productosCacheados)" -ForegroundColor Cyan
}

Write-Host "`n=== Pruebas completadas ===" -ForegroundColor Cyan
Write-Host "Notas:" -ForegroundColor Yellow
Write-Host "- Recuerda reemplazar REEMPLAZA_CON_JWT_TOKEN con un token JWT válido" -ForegroundColor Gray
Write-Host "- Los endpoints de caché no requieren autenticación (AllowAnonymous)" -ForegroundColor Gray
Write-Host "- Las pruebas miden rendimiento (HIT vs MISS)" -ForegroundColor Gray
