using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DiaryEntry {
    public string id;
    public string title;
    public string dateIso;
    public string situation;
    public string fact;
    public string emotion;
    public string thought;
    public long createdUnix;
    public long updatedUnix;
}

[Serializable]
public class DiaryCollection {
    public int version = 1;
    public List<DiaryEntry> entries = new();
}