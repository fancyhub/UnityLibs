package com.fancyhub;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.List;

class PermissionResult {
    public boolean allGranted;
    public List<String> grantedList;
    public List<String> deniedList;



    public String ToJsonString()
    {
        try{
            JSONObject jsonObject = new JSONObject();
            jsonObject.put("allGranted", allGranted);

            if(grantedList!=null)
            {
                JSONArray arrayGranted = new JSONArray();
                for(String g : grantedList)
                {
                    arrayGranted.put(g);
                }
                jsonObject.put("grantedList",arrayGranted);
            }

            if(deniedList!=null)
            {
                JSONArray arrayDenied = new JSONArray();
                for(String g : deniedList)
                {
                    arrayDenied.put(g);
                }
                jsonObject.put("deniedList",arrayDenied);
            }
            return jsonObject.toString();
        }catch (Exception ex)
        {
            return "";
        }
    }
}

public interface IPermissionResult {

    /**
     * 
     * @param result PermissionResult json ser
     */
    public void onResult(String result);
}

interface IPermissionInnerResult {
    /**
     * Callback for the permissions request result.
     *
     * @param allGranted  Indicate if all permissions that are granted.
     * @param grantedList All permissions that granted by user.
     * @param deniedList  All permissions that denied by user.
     */
    public void onResult(boolean allGranted, List<String> grantedList, List<String> deniedList);
}