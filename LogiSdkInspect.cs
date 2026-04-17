using System;
using Loupedeck;

class Program { 
  static void Main() { 
    Console.WriteLine("TouchEvents:"); 
    foreach (var n in Enum.GetNames(typeof(DeviceTouchEventType))) Console.WriteLine(n); 
  } 
}
