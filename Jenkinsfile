pipeline {
    agent { docker { image 'mcr.microsoft.com/dotnet/core/sdk:latest' } }
    environment {
        HOME = '/tmp'
    } 
    stages {
        stage ('Install dependencies')
        {
            steps
            {
                sh 'dotnet restore'
            }
        }
        stage ('Build')
        {
            steps
            {
                sh 'dotnet build'
            }
        }
        // stage ('Test')
        // {
        //     steps
        //     {
        //         sh 'dotnet test Api.Tests/Api.Tests.csproj'
        //     }
        // }
    }
}