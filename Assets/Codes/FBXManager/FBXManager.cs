using Meta.WitAi;
using Oculus.Interaction;
using Oculus.Interaction.Body.Input;
using Oculus.Interaction.HandGrab;
using OVRSimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Networking;

namespace TriLibCore.Samples
{
    [System.Serializable]
    public class SyncReq
    {
        public List<String> fish_list;
    }

    /// <summary>
    /// Demonstrates how to load a compressed (zipped) 3D model from a remote URL using TriLib.
    /// This class creates and configures loader options, downloads the model asynchronously,
    /// and reports progress or errors through callbacks.
    /// </summary>
    public class FBXManager : MonoBehaviour
    {

        /// <summary>
        /// The remote URL pointing to the zipped 3D model file.
        /// </summary>
        [Tooltip("URL of the compressed model file to load.")]
        public GameObject urlFBX;
        public string urlTail = ":5000/download";
        public float distanceFromCamera = 1.5f;
        private HashSet<string> hashList = new HashSet<string>();// ダウンロード済みの魚
        private string cacheDir;

        private bool isGenerating = false;
        public GameObject generatingDialog;


        /// <summary>
        /// Cached <see cref="AssetLoaderOptions"/> instance used to configure the model loading behavior.
        /// </summary>
        private AssetLoaderOptions _assetLoaderOptions;

        private void Start()
        {
            cacheDir = Path.Combine(Application.persistentDataPath, "ZipCache");
            Directory.CreateDirectory(cacheDir);
            LoadLocalHashes();

            //OnDownloadButtonClick();
        }

        private void Update()
        {
            generatingDialog.SetActive(isGenerating);
        }

        /// <summary>
        /// Unity’s Start method which is called on the frame when the script is enabled just before
        /// any of the Update methods are called for the first time.
        /// Creates default loader options if none are set, then begins downloading the model from the specified URL.
        /// </summary>
        /// <remarks>
        /// You can create and store a custom <see cref="AssetLoaderOptions"/> by right-clicking in the Assets folder
        /// and selecting <c>TriLib &gt; Create &gt; AssetLoaderOptions &gt; Pre-Built AssetLoaderOptions</c>.
        /// Alternatively, you can instantiate default options via <c>AssetLoader.CreateDefaultLoaderOptions(false, true)</c> 
        /// as demonstrated below.
        /// </remarks>
        public void OnDownloadButtonClick()
        {
            string url = "http://" + urlFBX.name + urlTail;
            if (!isGenerating)
            {
                isGenerating = true;
                StartCoroutine(DownloadFile(url));
            }
        }

        IEnumerator DownloadFile(string url)
        {
            UnityEngine.Debug.Log("Button is pressed.");

            // 持っている魚のハッシュ値を渡して，持っていない魚をダウンロード
            SyncReq req = new SyncReq
            {
                fish_list = hashList.ToList()
            };

            //Debug.Log("既に持っているリスト");
            //foreach(string hash in req.fish_list)
            //{
            //    Debug.Log(" " + hash);
            //}

            string jsonText = JsonUtility.ToJson(req);
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonText);
            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(postData);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                // 一匹もリストにいない場合
                if (hashList.Count <= 0)
                {
                    Debug.LogError("読み込みに失敗したため，終了");
                    isGenerating = false;
                    yield break;
                }

                // 読み込みに失敗した場合既存の魚から選出
                Debug.LogError("読み込みに失敗したため，既存の魚から選出します");
                SelectLocalZip();
                yield break;
            }
            if (request.responseCode == 204)
            {
                // 一匹もリストにいない場合
                if (hashList.Count <= 0)
                {
                    Debug.LogError("読み込みに失敗したため，終了");
                    isGenerating = false;
                    yield break;
                }

                // 新規ファイルがなければ既存の魚から選出
                Debug.LogError("新規の魚はいなかったため，既存の魚から選出します");
                SelectLocalZip();
                yield break;
            }
            else
            {
                // ダウンロードできたファイルからファイル名を保存
                // 保存が終わったらTriLib2でロード 
                UnityEngine.Debug.Log("ダウンロード成功");
                string fileName = GetFileName(request);

                if (string.IsNullOrEmpty(fileName))
                {
                    Debug.LogError("ファイル名取得失敗");
                    yield break;
                }

                string hash = Path.GetFileNameWithoutExtension(fileName);
                Debug.Log("取得したハッシュ：" + hash);

                string zipPath = Path.Combine(cacheDir, fileName);
                File.WriteAllBytes(zipPath, request.downloadHandler.data);

                hashList.Add(hash);
                Debug.Log($"保存完了: {fileName}");
                UseZip(zipPath);
            }
        }

        private string GetFileName(UnityWebRequest request)
        {
            foreach (var h in request.GetResponseHeaders())
            {
                Debug.Log($"{h.Key}: {h.Value}");
            }

            string disposition = request.GetResponseHeader("Content-Disposition");

            if (string.IsNullOrEmpty(disposition))
                return null;

            Match match = Regex.Match(
                disposition,
                @"filename=""?([^"";]+)""?");

            return match.Success
                ? match.Groups[1].Value
                : null;
        }

        private void LoadLocalHashes()
        {
            foreach (string file in Directory.GetFiles(cacheDir, "*.zip"))
            {
                string hash =
                    Path.GetFileNameWithoutExtension(file);

                hashList.Add(hash);
            }

            Debug.Log($"ローカルZIP数: {hashList.Count}");
        }

        private void SelectLocalZip()
        {
            if (hashList.Count == 0)
            {
                Debug.Log("ローカルZIPなし");
                return;
            }

            string hash =
                hashList.ElementAt(
                    UnityEngine.Random.Range(0, hashList.Count));

            string path =
                Path.Combine(cacheDir, hash + ".zip");

            Debug.Log($"ローカル選択: {hash}");

            UseZip(path);
        }
        private void UseZip(string zipPath)
        {
            isGenerating = false;
            Debug.Log($"魚を生成: {zipPath}");
            if (_assetLoaderOptions == null)
            {
                _assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
            }

            AssetLoader.LoadModelFromFile(
                zipPath,
                OnLoad,
                OnMaterialsLoad,
                OnProgress,
                OnError,
                null,
                _assetLoaderOptions);
        }

        /// <summary>
        /// Callback invoked when an error occurs during the download or loading process.
        /// Logs the detailed exception information for troubleshooting.
        /// </summary>
        /// <param name="obj">The contextualized error containing the original exception and any relevant context information.</param>
        private void OnError(IContextualizedError obj)
        {
            Debug.LogError($"An error occurred while loading your model: {obj.GetInnerException()}");
        }

        /// <summary>
        /// Callback invoked to report the current model loading progress.
        /// Use this to update UI or track loading status.
        /// </summary>
        /// <param name="assetLoaderContext">The context used by TriLib while loading the model.</param>
        /// <param name="progress">The current loading progress value between 0.0 and 1.0.</param>
        private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
        {
            Debug.Log($"Loading Model. Progress: {progress:P}");
        }

        /// <summary>
        /// Callback invoked once all model textures and materials have finished loading.
        /// At this stage, the <see cref="GameObject"/> is fully loaded and rendered.
        /// </summary>
        /// <remarks>
        /// The loaded <see cref="GameObject"/> can be accessed via <c>assetLoaderContext.RootGameObject</c>.
        /// </remarks>
        /// <param name="assetLoaderContext">The context containing loading details and the resultant GameObject.</param>
        private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            Debug.Log("All materials have been applied. The model is fully loaded.");

            // 魚のオブジェクト
            GameObject obj = assetLoaderContext.RootGameObject;
            SkinnedMeshRenderer smr = obj.GetComponentInChildren<SkinnedMeshRenderer>();

            obj.transform.Find("Armature").eulerAngles = new Vector3(-90, -90, 0);

            // アニメーション付与
            // ボーンの数が3つのときと4つのときで付与するアニメーションを変更
            RuntimeAnimatorController controller;
            if (smr.bones.Length < 4)
            {
                controller = Resources.Load<RuntimeAnimatorController>("MoveTailFor3Tails");
            }
            else
            {
                controller = Resources.Load<RuntimeAnimatorController>("MoveTailFor4Tails");
            }
            Animator anim = obj.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            // エージェント化
            obj.AddComponent<FishAgent>();


            // シェーダー変更
            foreach(var mat in smr.materials)
            {
                mat.shader = Shader.Find("Custom/CutOffOcclusionLit");
            }

            // コライダー付与
            smr.AddComponent<BoxCollider>();
            obj.AddComponent<BoxCollider>().Copy(smr.GetComponent<BoxCollider>());
            Destroy(smr.GetComponent<BoxCollider>());
            obj.GetComponent<BoxCollider>().isTrigger = true;
            obj.GetComponent<BoxCollider>().excludeLayers = LayerMask.GetMask("Player");

            obj.AddComponent<AddGrabbable>();

            // カメラの前に配置
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                Vector3 cameraForward = mainCamera.transform.forward;
                Vector3 spawnPosition = mainCamera.transform.position + cameraForward * distanceFromCamera;
                obj.transform.position = spawnPosition;
            }

            //if(hashList.Count < 10)
            //{
            //    OnDownloadButtonClick();
            //}
        }

        /// <summary>
        /// Callback invoked when the model’s meshes and hierarchy are loaded, but before textures and materials are applied.
        /// </summary>
        /// <remarks>
        /// The partially loaded <see cref="GameObject"/> can be accessed via <c>assetLoaderContext.RootGameObject</c>.
        /// </remarks>
        /// <param name="assetLoaderContext">The context containing loading details and the resultant GameObject.</param>
        private void OnLoad(AssetLoaderContext assetLoaderContext)
        {
            Debug.Log("Model mesh and hierarchy loaded successfully. Proceeding to load materials...");
        }
    }
}
