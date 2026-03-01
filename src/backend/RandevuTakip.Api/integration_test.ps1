$ErrorActionPreference = "Stop"

try {
    $loginBody = @{ email = "admin@demo.com"; password = "admin123" } | ConvertTo-Json
    $loginRes = Invoke-RestMethod -Uri "http://localhost:5032/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginRes.token

    $tenantSlug = "dentist"
    
    $services = Invoke-RestMethod -Uri "http://localhost:5032/api/$tenantSlug/services" -Method Get
    $genelMuayene = $services | Where-Object name -match "Genel Muayene" | Select-Object -First 1
    if (-not $genelMuayene) { throw "Service 'Genel Muayene' not found" }
    $serviceId = $genelMuayene.id

    # Monday Mar 2nd 2026
    $dateStr = "2026-03-02"

    Write-Output "--- Available Slots (Monday) ---"
    $slotsUrl = "http://localhost:5032/api/$tenantSlug/availability?serviceId=$serviceId&date=$dateStr"
    $slots = Invoke-RestMethod -Uri $slotsUrl -Method Get
    $slots | ConvertTo-Json -Depth 2 | Write-Output

    Write-Output "--- Booking 10:00 (Should go to Personel A) ---"
    $bookBody1 = @{
        serviceId     = $serviceId
        slotDate      = $dateStr
        slotTime      = "10:00"
        customerName  = "API Test Customer 1"
        customerPhone = "555111"
        customerEmail = "test1@test.com"
    } | ConvertTo-Json
    $bookRes1 = Invoke-RestMethod -Uri "http://localhost:5032/api/$tenantSlug/appointments" -Method Post -Body $bookBody1 -ContentType "application/json"
    $bookRes1 | ConvertTo-Json -Depth 2 | Write-Output

    Write-Output "--- Booking 14:00 (Should go to Personel B) ---"
    $bookBody2 = @{
        serviceId     = $serviceId
        slotDate      = $dateStr
        slotTime      = "14:00"
        customerName  = "API Test Customer 2"
        customerPhone = "555222"
        customerEmail = "test2@test.com"
    } | ConvertTo-Json
    $bookRes2 = Invoke-RestMethod -Uri "http://localhost:5032/api/$tenantSlug/appointments" -Method Post -Body $bookBody2 -ContentType "application/json"
    $bookRes2 | ConvertTo-Json -Depth 2 | Write-Output

    Write-Output "--- Final Assignments Verification ---"
    $apps = Invoke-RestMethod -Uri "http://localhost:5032/api/admin/appointments" -Method Get -Headers @{ "Authorization" = "Bearer $token" }
    
    # Get Staff Names for better reporting
    $staff = Invoke-RestMethod -Uri "http://localhost:5032/api/admin/staff" -Method Get -Headers @{ "Authorization" = "Bearer $token" }
    
    $results = $apps | Where-Object { $_.customerName -match "API Test" } | Select-Object customerName, appointmentTime, staffId | ForEach-Object {
        $sName = ($staff | Where-Object id -eq $_.staffId).name
        [PSCustomObject]@{
            Customer  = $_.customerName
            Time      = $_.appointmentTime
            StaffName = $sName
        }
    }
    $results | ConvertTo-Json -Depth 2 | Write-Output

}
catch {
    Write-Output "ERROR: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        Write-Output "RESPONSE BODY: $($reader.ReadToEnd())"
    }
}
