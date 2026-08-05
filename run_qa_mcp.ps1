function Call-Mcp ($method, $args) {
    $body = @{
        jsonrpc = "2.0"
        id = 1
        method = "tools/call"
        params = @{
            name = $method
            arguments = $args
        }
    } | ConvertTo-Json -Depth 5
    
    $response = Invoke-RestMethod -Uri "http://127.0.0.1:7269/mcp" -Method Post -Body $body -ContentType "application/json" -ErrorAction SilentlyContinue
    if ($response.result -and $response.result.content) {
        return $response.result.content[0].text
    }
    return $response | ConvertTo-Json -Depth 5
}

Write-Host "--- Stopping Play Mode ---"
Call-Mcp "play_stop" @{} | Out-Null
Start-Sleep -Seconds 2

Write-Host "--- Starting Play Mode ---"
Write-Host (Call-Mcp "play_start" @{})
Start-Sleep -Seconds 8

Write-Host "--- Assigning A02 ---"
Write-Host (Call-Mcp "console_command" @{ command = "ub_qa_assign_me apartment-a02" })
Start-Sleep -Seconds 1

Write-Host "--- Testing Stash ---"
Write-Host (Call-Mcp "console_command" @{ command = "ub_qa_test_stash apartment-a02" })
Start-Sleep -Seconds 1

Write-Host "--- Fixing Anchors QA ---"
Write-Host (Call-Mcp "console_command" @{ command = "ub_qa_fix_anchors" })
Start-Sleep -Seconds 1

Write-Host "--- Reading Console ---"
Write-Host (Call-Mcp "read_console" @{ lines = 30 })

Write-Host "--- Stopping Play Mode ---"
Call-Mcp "play_stop" @{} | Out-Null
