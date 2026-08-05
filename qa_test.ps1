try {
    Write-Host "Stopping Play Mode..."
    Invoke-RestMethod -Uri "http://127.0.0.1:7269/mcp/play_stop" -Method Post -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    Write-Host "Starting Play Mode..."
    Invoke-RestMethod -Uri "http://127.0.0.1:7269/mcp/play_start" -Method Post
    Start-Sleep -Seconds 5

    Write-Host "Assigning Apartment A02..."
    Invoke-RestMethod -Uri "http://127.0.0.1:7269/mcp/console_command" -Method Post -Body "ub_qa_assign_me apartment-a02"
    Start-Sleep -Seconds 2

    Write-Host "Testing Stash A02..."
    Invoke-RestMethod -Uri "http://127.0.0.1:7269/mcp/console_command" -Method Post -Body "ub_qa_test_stash apartment-a02"
    Start-Sleep -Seconds 2

    Write-Host "Fixing Anchors QA..."
    Invoke-RestMethod -Uri "http://127.0.0.1:7269/mcp/console_command" -Method Post -Body "ub_qa_fix_anchors"
    Start-Sleep -Seconds 2

    Write-Host "Reading Console..."
    $response = Invoke-RestMethod -Uri "http://127.0.0.1:7269/mcp/read_console" -Method Get
    Write-Host $response

    Write-Host "Stopping Play Mode..."
    Invoke-RestMethod -Uri "http://127.0.0.1:7269/mcp/play_stop" -Method Post
} catch {
    Write-Host "Error connecting to S&box MCP: $_"
}
