/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2020/5/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

namespace FH
{
    /// <summary>
    /// Error Code
    /// </summary>
    public enum EResError : int
    {
        OK = 0,

        //一些通用的
        Error = -1,
        PathNull = -2,
        ResMgrNotInit = -3,
        UserCancelled = -4,
        InUpgrading = -5,

        AssetPool_start = -1500,
        AssetPool_asset_null,
        AssetPool_e2,
        AssetPool_same_path_diff_asset,
        AssetPool_user_count_not_zero,
        AssetPool_user_null_1,
        AssetPool_user_null_2,
        AssetPool_user_null_3,        
        AssetPool_addusage_user_exist,
        AssetPool_path_null_1,
        AssetPool_path_null_2,
        AssetPool_asset_not_exist,
        AssetPool_asset_not_exist_2,
        AssetPool_asset_not_exist_3,
        AssetPool_asset_not_exist_4,
        AssetPool_asset_not_exist_5,
        AssetPool_asset_not_exist_6,
        AssetPool_asset_not_exist_7,
        AssetPool_user_not_exist,
        AssetPool_asset_is_not_spec_type,
        AssetPool_multi_path_to_one_asset,
        AssetPool_e3,
        AssetPool_e4,
        AssetPool_item_is_old_canot_add_user,
        AssetPool_dont_support_transfer_user,


        AssetLoaderAsync_start = -1100,
        AssetLoaderAsync_asset_not_exist,
        AssetLoaderAsync_load_asset_failed,
        AssetLoaderAsync_load_asset_failed2,

        AssetLoaderSync_start = -1200,
        AssetLoaderSync_load_asset_failed,

        GameObjectCreatorAsync_start = -1300,
        GameObjectCreatorAsync_inst_error_asset_null,
        GameObjectCreatorAsync_inst_error_asset_null2,
        GameObjectCreatorAsync_inst_error_unkown,
        GameObjectCreatorAsync_inst_error_not_gameobj,

        GameObjectCreatorSync_start = -1400,
        GameObjectCreatorSync_inst_error_asset_null,
        GameObjectCreatorSync_inst_error_not_gameobj,
        GameObjectCreatorSync_inst_error_unkown,

        ResMgrImplement_start = -1800,
        ResMgrImplement_path_null_2,
        ResMgrImplement_path_null_3,
        ResMgrImplement_path_null_4,
        ResMgrImplement_path_null_5,
        ResMgrImplement_path_null_6,
        ResMgrImplement_path_null_7,
        ResMgrImplement_path_null_8,
        ResMgrImplement_user_null_1,
        ResMgrImplement_asset_not_exist,
        ResMgrImplement_create_failed,
        ResMgrImplement_call_back_null,
        ResMgrImplement_call_back_null2,
        ResMgrImplement_call_back_null3,
        ResMgrImplement_res_canot_find,
        ResMgrImplement_inst_not_empty_obj,
        ResMgrImplement_inst_not_empty_obj2,

        GameObjectInstPool_start = -1900,
        GameObjectInstPool_user_null_1,
        GameObjectInstPool_user_null_2,
        GameObjectInstPool_user_null_3,
        GameObjectInstPool_user_null_4,
        GameObjectInstPool_user_null_5,
        GameObjectInstPool_pool_val_not_found_1,
        GameObjectInstPool_pool_val_not_found_2,
        GameObjectInstPool_pool_val_not_found_3,
        GameObjectInstPool_pool_val_not_found_4,
        GameObjectInstPool_canot_add_user_2_nowait_inst,
        GameObjectInstPool_add_user_err,
        GameObjectInstPool_user_remove_twice,
        GameObjectInstPool_user_remove_not_same_user,
        GameObjectInstPool_path_null_1,
        GameObjectInstPool_no_free_inst,

        GameObjectPreInstData_start = -2100,
        GameObjectPreInstData_path_null_1,
        GameObjectPreInstData_count_zero,
        GameObjectPreInstData_req_id_not_exist,
        GameObjectPreInstData_cant_find_data_with_path,

        EmptyGameObjectPool_start = -2200,
        EmptyGameObjectPool_obj_null,
        EmptyGameObjectPool_obj_not_empty,
        EmptyGameObjectPool_push_twice,
        EmptyGameObjectPool_id_is_not_in_using,
        EmptyGameObjectPool_id_not_exist,
        EmptyGameObjectPool_obj_destroy_outer,
        EmptyGameObjectPool_obj_destroy_outer2,
        EmptyGameObjectPool_user_null,
        EmptyGameObjectPool_user_already_exist,
        EmptyGameObjectPool_user_not_exist,
    }
}
