[CmdletBinding()]
param(
    [string[]]$Arguments
)

if (-not $Arguments -or $Arguments.Count -eq 0)
{
    $Arguments = @("test")
}

$timeout = [TimeSpan]::FromMinutes(2)
$timeoutSeconds = [int]$timeout.TotalSeconds
$timeoutMilliseconds = [int]$timeout.TotalMilliseconds

Write-Host "Running: dotnet $([string]::Join(' ', $Arguments)) (timeout ${timeoutSeconds}s)"

$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = "dotnet"
$startInfo.UseShellExecute = $false
$startInfo.RedirectStandardOutput = $false
$startInfo.RedirectStandardError = $false
$startInfo.RedirectStandardInput = $false
$startInfo.CreateNoWindow = $false

foreach ($arg in $Arguments)
{
    [void]$startInfo.ArgumentList.Add($arg)
}

$process = [System.Diagnostics.Process]::Start($startInfo)

if (-not $process)
{
    Write-Error "Failed to start the dotnet process."
    exit 1
}

if (-not $process.WaitForExit($timeoutMilliseconds))
{
    Write-Warning "dotnet process (Id: $($process.Id)) exceeded ${timeoutSeconds}s. Terminating."
    try
    {
        $process.Kill($true)
    }
    catch
    {
        try
        {
            $process.Kill()
        }
        catch
        {
            Write-Warning "Failed to terminate dotnet process: $_"
        }
    }

    $process.WaitForExit()
    exit 124
}

$process.WaitForExit()
exit $process.ExitCode
