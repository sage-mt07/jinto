using System;
using System.IO;
using System.Text.RegularExpressions;

static string GetDocValue(string docText, string setting)
{
    var pattern = $"| {Regex.Escape(setting)} |([^|]+)|";
    var match = Regex.Match(docText, pattern);
    return match.Success ? match.Groups[1].Value.Trim() : "<missing>";
}

static string GetCodeValue(string filePath, string property)
{
    var text = File.ReadAllText(filePath);
    var pattern = $"{Regex.Escape(property)}.*=\s*([^;]+);";
    var match = Regex.Match(text, pattern);
    return match.Success ? match.Groups[1].Value.Trim() : "<missing>";
}

var docs = File.ReadAllText(Path.Combine("docs", "defaults.md"));

void Check(string file, string prop, string docKey)
{
    var expected = GetDocValue(docs, docKey);
    var actual = GetCodeValue(file, prop);
    if (expected != actual)
    {
        Console.WriteLine($"Mismatch {prop}: doc={expected} code={actual}");
    }
    else
    {
        Console.WriteLine($"OK {prop} = {actual}");
    }
}

// TopicAttribute defaults
Check(Path.Combine("src","Core","Abstractions","TopicAttribute.cs"), "PartitionCount", "PartitionCount");
Check(Path.Combine("src","Core","Abstractions","TopicAttribute.cs"), "ReplicationFactor", "ReplicationFactor");
Check(Path.Combine("src","Core","Abstractions","TopicAttribute.cs"), "RetentionMs", "RetentionMs");

// Producer defaults
Check(Path.Combine("src","Messaging","Configuration","ProducerSection.cs"), "Acks", "Producer.Acks");
Check(Path.Combine("src","Messaging","Configuration","ProducerSection.cs"), "CompressionType", "Producer.CompressionType");
Check(Path.Combine("src","Messaging","Configuration","ProducerSection.cs"), "EnableIdempotence", "Producer.EnableIdempotence");

// Consumer defaults
Check(Path.Combine("src","Messaging","Configuration","ConsumerSection.cs"), "AutoOffsetReset", "Consumer.AutoOffsetReset");
Check(Path.Combine("src","Messaging","Configuration","ConsumerSection.cs"), "EnableAutoCommit", "Consumer.EnableAutoCommit");
Check(Path.Combine("src","Messaging","Configuration","ConsumerSection.cs"), "AutoCommitIntervalMs", "Consumer.AutoCommitIntervalMs");

// DLQ defaults
Check(Path.Combine("src","Configuration","DlqTopicConfiguration.cs"), "RetentionMs", "RetentionMs");
Check(Path.Combine("src","Configuration","DlqTopicConfiguration.cs"), "NumPartitions", "NumPartitions");
Check(Path.Combine("src","Configuration","DlqTopicConfiguration.cs"), "ReplicationFactor", "ReplicationFactor");
Check(Path.Combine("src","Configuration","DlqTopicConfiguration.cs"), "EnableAutoCreation", "EnableAutoCreation");

// Window defaults
Check(Path.Combine("src","Core","Abstractions","IWindowedEntitySet.cs"), "WindowType", "WindowType");
Check(Path.Combine("src","Core","Abstractions","IWindowedEntitySet.cs"), "GracePeriod", "GracePeriod");
Check(Path.Combine("src","Core","Abstractions","IWindowedEntitySet.cs"), "OutputMode", "OutputMode");
Check(Path.Combine("src","Core","Abstractions","IWindowedEntitySet.cs"), "UseHeartbeat", "UseHeartbeat");

// ErrorHandling defaults
Check(Path.Combine("src","Messaging","Internal","ErrorHandlingContext.cs"), "ErrorAction", "ErrorAction");
Check(Path.Combine("src","Messaging","Internal","ErrorHandlingContext.cs"), "RetryCount", "RetryCount");
Check(Path.Combine("src","Messaging","Internal","ErrorHandlingContext.cs"), "RetryInterval", "RetryInterval");
