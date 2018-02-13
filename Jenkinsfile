pipeline {
  agent none
  stages {
    stage('testing stage') {
      steps {
        cleanWs(skipWhenFailed: true)
      }
    }
    stage('deploying') {
      steps {
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