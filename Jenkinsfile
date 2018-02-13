pipeline {
  agent none
  stages {
    stage('testing stage') {
      steps {
        powershell(script: 'blablabla.ps1', encoding: 'UTF-8', returnStatus: true)
      }
    }
    stage('deploying') {
      steps {
        powershell(script: 'getstuffDone', encoding: 'publishtest', returnStatus: true)
        cleanWs(skipWhenFailed: true)
      }
    }
    stage('Success') {
      steps {
        sleep 10
      }
    }
  }
  environment {
    Test = 'test'
  }
}