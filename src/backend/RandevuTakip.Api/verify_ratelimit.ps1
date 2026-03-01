$url = "http://localhost:5032/api/dentist/appointments"
$body = "{""serviceId"":""b1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"",""slotDate"":""2026-03-05"",""slotTime"":""10:00"",""customerName"":""Limiter Test"",""customerPhone"":""05001112233""}"

for ($idx = 1; $idx -le 7; $idx++) {
    Write-Host "Sent request $idx"
    try {
        $null = Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json"
        Write-Host "Sent Request ${idx}: Success (200 OK)" -ForegroundColor Green
    }
    catch {
        Write-Host "Sent Request ${idx}: Failed - $($_.Exception.Message)" -ForegroundColor Red
    }
}
