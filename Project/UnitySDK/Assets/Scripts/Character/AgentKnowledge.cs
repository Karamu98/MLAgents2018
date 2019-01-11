using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This simple struct will allow us to store data with a time stamp
public class AgentEvent
{
    public float timeStamp; // Stores when this information was found
    public float expiryTime; // Stores the time that this information expires
    public object value; // Stores the value (bool, float)
    public string keyValue; // Stores the key that goap will use

    public AgentEvent(string a_keyValue, object a_value, float a_timeStamp, float a_expireTime)
    {
        timeStamp = a_timeStamp;
        expiryTime = a_expireTime;
        value = a_value;
        keyValue = a_keyValue;
        return;
    }

    // Called to test if the information is still valid
    public bool IsValid()
    {
        if (expiryTime - Time.time < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool operator==(AgentEvent a_1, AgentEvent a_2)
    {
        if(a_1.keyValue == a_2.keyValue &&
            a_1.value.Equals(a_2.value))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool Equals(System.Object other)
    {
        if (this == null)
        {
            return false;
        }

        AgentEvent a_1 = (AgentEvent)other;

        if(other == null)
        {
            return false;
        }

        if(keyValue == a_1.keyValue
            && value == a_1.value
            && expiryTime == a_1.expiryTime
            && timeStamp == a_1.timeStamp)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        int hash = keyValue.GetHashCode() + value.GetHashCode() + expiryTime.GetHashCode() + timeStamp.GetHashCode();
        return hash;
    }

    public static bool operator!=(AgentEvent a_1, AgentEvent a_2)
    {
        if(a_1.keyValue != a_2.keyValue ||
    !a_1.value.Equals(a_2.value))
        {
            return false;
        }
        else
        {
            return true;
        }
    }


}

public class AgentKnowledge
{
    private List<AgentEvent> knowledgeBase;

    public AgentKnowledge()
    {
        knowledgeBase = new List<AgentEvent>();
    }

    public void AddKnowledge(AgentEvent a_newEvent)
    {
        knowledgeBase.Add(a_newEvent);
    }

    public List<AgentEvent> GetKnowledge()
    {
        return knowledgeBase;
    }

    public void SetEvent(string a_key, object a_newValue, float a_durationInSeconds)
    {
        int foundItr = -1;

        for(int i = 0; i < knowledgeBase.Count; i++)
        {
            if(knowledgeBase[i].keyValue == a_key)
            {
                foundItr = i;
                break;
            }
        }

        float fTimeStamp;

        if (a_durationInSeconds > 0)
        {
            fTimeStamp = Time.time + a_durationInSeconds;
        }
        else
        {
            fTimeStamp = 0;
        }


        knowledgeBase[foundItr] = new AgentEvent(knowledgeBase[foundItr].keyValue, a_newValue, Time.time, fTimeStamp);
    }

    public AgentEvent GetEvent(string a_key)
    {
        for(int i = 0; i < knowledgeBase.Count; i++)
        {
            if (knowledgeBase[i].keyValue == a_key)
            {
                return knowledgeBase[i];
            }
        }
        return new AgentEvent("", 0, 0, 0);
    }
}
