![logo-white](https://user-images.githubusercontent.com/30018771/178152345-49f6e952-d8f9-4999-96ac-682ff81641e0.png)

# Stepping.NET
Stepping is a distributed [BASE](https://en.wikipedia.org/wiki/Eventual_consistency) jobs implementation. You can use it as a workflow engine, event outbox/inbox, and more. 

The distributed transaction is based on DTM's [2-phase messaging](https://en.dtm.pub/practice/msg.html) pattern.

## What is `Job` and `Step` in Stepping?

`Job` is a distributed transaction unit, and `Step` is a specific task inside a job. A job contains some steps and finally executes them in order. If step 1 fails, it will be retried until success, and then step 2 starts to execute.

If a job involves a DB transaction, the steps will be **ensured to be done** after the transaction commit. You don't need to worry about inconsistencies caused by the app crashes after the transaction commit but before the steps are executed.

## Examples

Todo.

## Supported Transaction Managers

Stepping requires transaction managers. You can choose an implementation you like.

### DTM Server

DTM Server is a mature transaction manager that can be used with Stepping. By using DTM, you can use many other distributed transaction modes like SAGA, TCC, and XA.

See DTM's [document](https://en.dtm.pub/guide/install.html) to learn how to install the DTM Server.

### Stepping Self-TM

Stepping providers a Self-TM implementation for decentralization transaction managers. In short, who starts a job should be the TM of this job.
