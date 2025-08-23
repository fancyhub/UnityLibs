package com.fancyhub;

public interface IAlertDialogResult {
    /**
     * Call it when the user agrees or refuses to allow these permissions in
     * settings.
     *
     * @param isAgree agrees or refuses
     */
    public void onResult(boolean isAgree);
}