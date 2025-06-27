#!/usr/bin/env python3
import os
import re

def get_doc_value(text, key):
    # Special handling for ErrorHandlingContext defaults described in sentence form
    if key in {"ErrorAction", "RetryCount", "RetryInterval"}:
        m = re.search(r"ErrorHandlingContext[^`]*`?[^E]*ErrorAction=([^`,]+)`,\s*`RetryCount=([^`,]+)`,\s*`RetryInterval=([^`]+)`", text)
        if m:
            mapping = {
                "ErrorAction": m.group(1).strip(),
                "RetryCount": m.group(2).strip(),
                "RetryInterval": m.group(3).strip(),
            }
            val = mapping[key]
            if '(' in val:
                val = val.split('(')[0].strip()
            val = val.replace('秒', '').strip()
            val = re.sub(r'[^0-9A-Za-z_.-]', '', val)
            return val.strip('"').strip('`')
        return '<missing>'
    section = None
    if key.startswith('DLQ.'):
        key = key.split('.',1)[1]
        section = 'DLQ'
    pattern = rf"\| {re.escape(key)} \|([^|]+)\|"
    if section:
        sec_match = re.search(rf"## {section}構成(.*?)##", text, re.S)
        search_area = sec_match.group(1) if sec_match else text
    else:
        search_area = text
    m = re.search(pattern, search_area)
    if not m:
        return '<missing>'
    val = m.group(1).strip()
    if '(' in val:
        val = val.split('(')[0].strip()
    val = val.replace('秒', '').strip()
    val = re.sub(r'[^0-9A-Za-z_.-]', '', val)
    return val.strip('"').strip('`')

def get_code_value(path, prop):
    with open(path, encoding='utf-8') as f:
        text = f.read()
    m = re.search(rf"{re.escape(prop)}.*=\s*([^;]+);", text)
    if not m:
        return '<missing>'
    val = m.group(1).strip()
    val = val.strip('"')
    if val.startswith("TimeSpan.FromSeconds("):
        val = val.replace("TimeSpan.FromSeconds(", "").rstrip(")")
    if val.startswith("WindowType."):
        val = val.split('.',1)[1]
    if val.startswith("WindowOutputMode."):
        val = val.split('.',1)[1]
    if val.startswith("ErrorAction."):
        val = val.split('.',1)[1]
    return val

def check(file_path, prop, doc_key):
    expected = get_doc_value(docs, doc_key)
    actual = get_code_value(file_path, prop)
    if expected != actual:
        return f"Mismatch {prop}: doc={expected} code={actual}"
    return f"OK {prop} = {actual}"

docs = open(os.path.join('docs','defaults.md'), encoding='utf-8').read()

checks = [
    ('src/Core/Abstractions/TopicAttribute.cs','PartitionCount','PartitionCount'),
    ('src/Core/Abstractions/TopicAttribute.cs','ReplicationFactor','ReplicationFactor'),
    ('src/Core/Abstractions/TopicAttribute.cs','RetentionMs','RetentionMs'),
    ('src/Messaging/Configuration/ProducerSection.cs','Acks','Producer.Acks'),
    ('src/Messaging/Configuration/ProducerSection.cs','CompressionType','Producer.CompressionType'),
    ('src/Messaging/Configuration/ProducerSection.cs','EnableIdempotence','Producer.EnableIdempotence'),
    ('src/Messaging/Configuration/ConsumerSection.cs','AutoOffsetReset','Consumer.AutoOffsetReset'),
    ('src/Messaging/Configuration/ConsumerSection.cs','EnableAutoCommit','Consumer.EnableAutoCommit'),
    ('src/Messaging/Configuration/ConsumerSection.cs','AutoCommitIntervalMs','Consumer.AutoCommitIntervalMs'),
    ('src/Configuration/DlqTopicConfiguration.cs','RetentionMs','DLQ.RetentionMs'),
    ('src/Configuration/DlqTopicConfiguration.cs','NumPartitions','NumPartitions'),
    ('src/Configuration/DlqTopicConfiguration.cs','ReplicationFactor','ReplicationFactor'),
    ('src/Configuration/DlqTopicConfiguration.cs','EnableAutoCreation','EnableAutoCreation'),
    ('src/Core/Abstractions/IWindowedEntitySet.cs','WindowType','WindowType'),
    ('src/Core/Abstractions/IWindowedEntitySet.cs','GracePeriod','GracePeriod'),
    ('src/Core/Abstractions/IWindowedEntitySet.cs','OutputMode','OutputMode'),
    ('src/Core/Abstractions/IWindowedEntitySet.cs','UseHeartbeat','UseHeartbeat'),
    ('src/Messaging/Internal/ErrorHandlingContext.cs','ErrorAction','ErrorAction'),
    ('src/Messaging/Internal/ErrorHandlingContext.cs','RetryCount','RetryCount'),
    ('src/Messaging/Internal/ErrorHandlingContext.cs','RetryInterval','RetryInterval'),
]

results = [check(f,p,k) for f,p,k in checks]

mism = [r for r in results if r.startswith('Mismatch')]
print('# Config Defaults Validation Log')
print()
print(f'- Total checks: {len(checks)}')
if mism:
    print(f'- Result: {len(mism)} mismatches found')
else:
    print('- Result: No mismatches found.')
print()
for r in results:
    print(r)
