# VirtualMachineLifecycle

## Run

```bash

dotnet run -- --data-provider Sqlite --connection-string "Data Source=/Users/joshuang/Desktop/Programming/Ivy/VirtualMachineLifecycle/db.sqlite" --seed-database --yes-to-all

```

## Schema

```dbml

Enum user_type {
    viewer
    operator
}

Enum vm_status {
    running
    stopped
}

Enum provider {
    aws
    azure
}

Enum audit_action {
    login
    logout
    snapshot_created
    snapshot_deleted
    snapshot_restored
    vm_start
    vm_stop
    reboot
    delete
    view
}

Table user {
    id int [pk, increment]
    username varchar [not null]
    type user_type [not null]
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Table vm {
    id int [pk, increment]
    name varchar [not null]
    status vm_status [not null]
    provider provider [not null]
    tags text
    last_action timestamp
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Table snapshot {
    id int [pk, increment]
    vm_id int [not null]
    created_by int [not null]
    name varchar [not null]
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Table audit_log {
    id int [pk, increment]
    user_id int [not null]
    vm_id int
    action audit_action [not null]
    timestamp timestamp [not null]
    details varchar
}

Ref: snapshot.vm_id > vm.id
Ref: snapshot.created_by > user.id
Ref: audit_log.user_id > user.id
Ref: audit_log.vm_id > vm.id

```

## Prompt

```
Virtual Machine Lifecycle 

2 user types: Viewer and Operator

VM: Name, Status (running or Stopped), Provider (I.e. AWS, Azure), tags, Last action

If Operator logged in, also possible to select/unselect one or multiple VMs and Start, Stop or Reboot

Also possible as Operator to Create a Snapshot of a VM, and restore that snapshot or else delete it

Audit Log: Log of actions that can be filtered by user, action (login, logout, Snapshot created, Snapshot deleted, snapshot restored, VM Start, VM Stop, Reboot, Delete, View)


```
