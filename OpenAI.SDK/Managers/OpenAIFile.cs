﻿using System.Net.Http.Json;
using Betalgo.Ranul.OpenAI.Extensions;
using Betalgo.Ranul.OpenAI.Interfaces;
using Betalgo.Ranul.OpenAI.ObjectModels.ResponseModels.FileResponseModels;
using Betalgo.Ranul.OpenAI.ObjectModels.SharedModels;

namespace Betalgo.Ranul.OpenAI.Managers;

#pragma warning disable CS1591
public partial class OpenAIService : IFileService
#pragma warning restore CS1591
{
    /// <inheritdoc />
    public async Task<FileListResponse> ListFile(CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<FileListResponse>(_endpointProvider.FilesList(), cancellationToken))!;
    }

    /// <inheritdoc />
    public async Task<FileUploadResponse> UploadFile(string purpose, byte[] file, string fileName, CancellationToken cancellationToken = default)
    {
        var multipartContent = new MultipartFormDataContent
        {
            { new StringContent(purpose), "purpose" },
            { new ByteArrayContent(file), "file", fileName }
        };

        return await _httpClient.PostFileAndReadAsAsync<FileUploadResponse>(_endpointProvider.FilesUpload(), multipartContent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FileDeleteResponse> DeleteFile(string fileId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.DeleteAndReadAsAsync<FileDeleteResponse>(_endpointProvider.FileDelete(fileId), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FileResponse> RetrieveFile(string fileId, CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<FileResponse>(_endpointProvider.FileRetrieve(fileId), cancellationToken))!;
    }

    /// <inheritdoc />
    public async Task<FileContentResponse<T?>> RetrieveFileContent<T>(string fileId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(_endpointProvider.FileRetrieveContent(fileId), cancellationToken);

        if (typeof(T) == typeof(string))
        {
            return new()
            {
                Content = (T)(object)await response.Content.ReadAsStringAsync(cancellationToken)
            };
        }

        if (typeof(T) == typeof(byte[]))
        {
            return new()
            {
                Content = (T)(object)await response.Content.ReadAsByteArrayAsync(cancellationToken)
            };
        }

        if (typeof(T) == typeof(Stream))
        {
            return new()
            {
                Content = (T)(object)await response.Content.ReadAsStreamAsync(cancellationToken)
            };
        }

        return new()
        {
            Content = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
        };
    }
}