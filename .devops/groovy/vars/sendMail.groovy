def call(body){
    def config = [:]
        body.resolveStrategy = Closure.DELEGATE_FIRST
        body.delegate = config
        body()


def emailSubject = "${config.buildStatus} | Integration Build ${env.GIT_NEW_BRANCH_NAME}"
def emailBody = config.emailBody


emailext attachLog: true, body: emailBody, recipientProviders: [[$class: 'CulpritsRecipientProvider'], [$class: 'DevelopersRecipientProvider'], [$class: 'RequesterRecipientProvider'], [$class: 'UpstreamComitterRecipientProvider']], subject: emailSubject, to: 'wennis@ipipeline.com'
}